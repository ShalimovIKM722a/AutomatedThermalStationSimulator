using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class AttorchCoulometer
{
    private const int DefaultPort = 6668;

    private const uint CommandHandshakeStart = 0x03;
    private const uint CommandHandshakeResponse = 0x04;
    private const uint CommandHandshakeFinish = 0x05;
    private const uint CommandStatus = 0x10;

    private static readonly byte[] Prefix6699 =
    {
        0x00, 0x00, 0x66, 0x99
    };

    private static readonly byte[] Suffix6699 =
    {
        0x00, 0x00, 0x99, 0x66
    };

    private readonly string _ipAddress;
    private readonly string _localKey;
    private readonly int _port;

    private TcpClient? _client;
    private NetworkStream? _stream;
    private byte[]? _sessionKey;

    private uint _sequence = 1;

    public bool IsConnected =>
        _client?.Connected == true;

    public double Voltage { get; private set; }

    public double ChargeCurrent { get; private set; }

    public double DischargeCurrent { get; private set; }

    public double Current { get; private set; }

    public double Capacity { get; private set; }

    public double Temperature { get; private set; }

    public string ShuntType { get; private set; } = "unknown";

    public Dictionary<string, JsonElement> Dps { get; private set; } = new();

    public AttorchCoulometer(
        string ipAddress,
        string localKey,
        int port = DefaultPort)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException(
                "IP address is required.",
                nameof(ipAddress));

        if (string.IsNullOrWhiteSpace(localKey))
            throw new ArgumentException(
                "Local Key is required.",
                nameof(localKey));

        byte[] keyBytes =
            Encoding.UTF8.GetBytes(localKey);

        if (keyBytes.Length != 16)
        {
            throw new ArgumentException(
                "Local Key must be exactly 16 bytes.",
                nameof(localKey));
        }

        _ipAddress = ipAddress;
        _localKey = localKey;
        _port = port;
    }

    public async Task ReadStatusAsync()
    {
        await ConnectAsync();

        try
        {
            await HandshakeAsync();
            await RequestStatusAsync();
        }
        finally
        {
            Console.WriteLine("Turn on attorch cw24");
            Disconnect();
        }
    }

    public async Task ConnectAsync()
    {
        if (IsConnected)
            return;

        _client = new TcpClient();

        _client.NoDelay = true;

        await _client.ConnectAsync(
            _ipAddress,
            _port);

        _stream = _client.GetStream();
    }

    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();

        _stream = null;
        _client = null;
        _sessionKey = null;
    }

    private async Task HandshakeAsync()
    {
        if (_stream == null)
        {
            throw new InvalidOperationException(
                "Connection is not established.");
        }

        byte[] localKeyBytes =
            Encoding.UTF8.GetBytes(_localKey);

        byte[] clientNonce =
            RandomNumberGenerator.GetBytes(16);

        await SendFrameAsync(
            _stream,
            _sequence++,
            CommandHandshakeStart,
            clientNonce,
            localKeyBytes);

        TuyaFrame response =
            await ReadFrameAsync(
                _stream,
                localKeyBytes);

        if (response.Command != CommandHandshakeResponse)
        {
            throw new IOException(
                $"Expected handshake response 0x04, " +
                $"received 0x{response.Command:X}.");
        }

        byte[] payload =
            response.Payload;

        if (payload.Length < 52)
        {
            throw new IOException(
                "Handshake payload is too short.");
        }

        uint retCode =
            ReadUInt32BE(
                payload,
                0);

        if (retCode != 0)
        {
            throw new IOException(
                $"Device returned handshake error: " +
                $"retcode={retCode}.");
        }

        byte[] deviceNonce =
            payload[4..20];

        byte[] receivedHmac =
            payload[20..52];

        using HMACSHA256 hmac =
            new HMACSHA256(localKeyBytes);

        byte[] expectedHmac =
            hmac.ComputeHash(clientNonce);

        if (!CryptographicOperations.FixedTimeEquals(
                receivedHmac,
                expectedHmac))
        {
            throw new CryptographicException(
                "Client nonce HMAC does not match.");
        }

        byte[] finishHmac =
            hmac.ComputeHash(deviceNonce);

        await SendFrameAsync(
            _stream,
            _sequence++,
            CommandHandshakeFinish,
            finishHmac,
            localKeyBytes);

        await Task.Delay(10);

        _sessionKey =
            DeriveSessionKey(
                clientNonce,
                deviceNonce,
                localKeyBytes);

        if (_sessionKey.Length != 16)
        {
            throw new CryptographicException(
                "Invalid session key length.");
        }
    }

    private async Task RequestStatusAsync()
    {
        if (_stream == null)
        {
            throw new InvalidOperationException(
                "Connection is not established.");
        }

        if (_sessionKey == null)
        {
            throw new InvalidOperationException(
                "Session key is not initialized.");
        }

        byte[] payload =
            Encoding.UTF8.GetBytes("{}");

        await SendFrameAsync(
            _stream,
            _sequence++,
            CommandStatus,
            payload,
            _sessionKey);

        await Task.Delay(10);

        TuyaFrame response =
            await ReadFrameAsync(
                _stream,
                _sessionKey);

        ParseStatus(
            response.Payload);
    }

    private void ParseStatus(byte[] payload)
    {
        if (payload.Length < 4)
        {
            throw new IOException(
                "Response is too short for retcode.");
        }

        uint retCode =
            ReadUInt32BE(
                payload,
                0);

        if (retCode != 0)
        {
            throw new IOException(
                $"CW24 returned retcode={retCode}.");
        }

        byte[] data =
            payload[4..];

        byte[] versionHeader =
        {
            (byte)'3',
            (byte)'.',
            (byte)'5',
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };

        if (data.Length >= 15 &&
            data[..15]
                .AsSpan()
                .SequenceEqual(versionHeader))
        {
            data = data[15..];
        }

        string json =
            Encoding.UTF8.GetString(data);

        using JsonDocument document =
            JsonDocument.Parse(json);

        JsonElement root =
            document.RootElement;

        JsonElement dps;

        if (root.TryGetProperty(
                "dps",
                out dps))
        {
            ParseDps(dps);
            return;
        }

        if (root.TryGetProperty(
                "data",
                out JsonElement dataElement))
        {
            if (dataElement.ValueKind ==
                    JsonValueKind.Object &&
                dataElement.TryGetProperty(
                    "dps",
                    out dps))
            {
                ParseDps(dps);
                return;
            }
        }

        throw new IOException(
            "DPS not found in device response.");
    }

    private void ParseDps(JsonElement dps)
    {
        Dps = new Dictionary<string, JsonElement>();

        foreach (JsonProperty property
                 in dps.EnumerateObject())
        {
            Dps[property.Name] =
                property.Value.Clone();
        }

        Voltage = 0;
        ChargeCurrent = 0;
        DischargeCurrent = 0;
        Current = 0;
        Capacity = 0;
        Temperature = 0;
        ShuntType = "unknown";

        if (TryGetDouble(
                Dps,
                "20",
                out double rawVoltage))
        {
            Voltage =
                rawVoltage / 100.0;
        }

        if (TryGetDouble(
                Dps,
                "18",
                out double rawCharge))
        {
            ChargeCurrent =
                rawCharge / 100.0;
        }

        if (TryGetDouble(
                Dps,
                "19",
                out double rawDischarge))
        {
            DischargeCurrent =
                rawDischarge / 100.0;
        }

        Current =
            ChargeCurrent > 0
                ? ChargeCurrent
                : -DischargeCurrent;

        if (TryGetDouble(
                Dps,
                "133",
                out double rawCapacity))
        {
            Capacity =
                rawCapacity / 1000.0;
        }

        if (TryGetDouble(
                Dps,
                "135",
                out double rawTemperature))
        {
            Temperature =
                rawTemperature;
        }

        if (Dps.TryGetValue(
                "136",
                out JsonElement shunt))
        {
            ShuntType =
                shunt.ToString();
        }
    }

    private static byte[] DeriveSessionKey(
        byte[] clientNonce,
        byte[] deviceNonce,
        byte[] localKey)
    {
        if (clientNonce.Length != 16)
        {
            throw new ArgumentException(
                "Client nonce must be 16 bytes.");
        }

        if (deviceNonce.Length != 16)
        {
            throw new ArgumentException(
                "Device nonce must be 16 bytes.");
        }

        byte[] xor =
            new byte[16];

        for (int i = 0; i < 16; i++)
        {
            xor[i] =
                (byte)(
                    clientNonce[i] ^
                    deviceNonce[i]);
        }

        byte[] iv =
            clientNonce[..12];

        byte[] encrypted =
            AesGcmEncrypt(
                localKey,
                iv,
                xor,
                Array.Empty<byte>());

        if (encrypted.Length != 32)
        {
            throw new CryptographicException(
                "Invalid session derivation length.");
        }

        return encrypted[..16];
    }

    private static async Task SendFrameAsync(
        NetworkStream stream,
        uint sequence,
        uint command,
        byte[] payload,
        byte[] key)
    {
        byte[] iv =
            RandomNumberGenerator.GetBytes(12);

        uint length =
            (uint)(
                payload.Length +
                12 +
                16);

        byte[] header =
            new byte[18];

        Buffer.BlockCopy(
            Prefix6699,
            0,
            header,
            0,
            4);

        header[4] = 0;
        header[5] = 0;

        WriteUInt32BE(
            header,
            6,
            sequence);

        WriteUInt32BE(
            header,
            10,
            command);

        WriteUInt32BE(
            header,
            14,
            length);

        byte[] aad =
            header[4..18];

        byte[] encrypted =
            AesGcmEncrypt(
                key,
                iv,
                payload,
                aad);

        byte[] frame =
            new byte[
                header.Length +
                iv.Length +
                encrypted.Length +
                Suffix6699.Length];

        int offset = 0;

        Buffer.BlockCopy(
            header,
            0,
            frame,
            offset,
            header.Length);

        offset +=
            header.Length;

        Buffer.BlockCopy(
            iv,
            0,
            frame,
            offset,
            iv.Length);

        offset +=
            iv.Length;

        Buffer.BlockCopy(
            encrypted,
            0,
            frame,
            offset,
            encrypted.Length);

        offset +=
            encrypted.Length;

        Buffer.BlockCopy(
            Suffix6699,
            0,
            frame,
            offset,
            Suffix6699.Length);

        await stream.WriteAsync(
            frame,
            0,
            frame.Length);

        await stream.FlushAsync();
    }

    private static async Task<TuyaFrame> ReadFrameAsync(
        NetworkStream stream,
        byte[] key)
    {
        byte[] header =
            await ReadExactAsync(
                stream,
                18);

        if (!header[..4]
                .AsSpan()
                .SequenceEqual(Prefix6699))
        {
            throw new IOException(
                "Invalid Tuya prefix.");
        }

        uint sequence =
            ReadUInt32BE(
                header,
                6);

        uint command =
            ReadUInt32BE(
                header,
                10);

        uint length =
            ReadUInt32BE(
                header,
                14);

        if (length < 28)
        {
            throw new IOException(
                $"Invalid frame length: {length}.");
        }

        if (length > 1024 * 1024)
        {
            throw new IOException(
                "Frame is too large.");
        }

        byte[] body =
            await ReadExactAsync(
                stream,
                checked((int)length));

        byte[] footer =
            await ReadExactAsync(
                stream,
                4);

        if (!footer
                .AsSpan()
                .SequenceEqual(Suffix6699))
        {
            throw new IOException(
                "Invalid Tuya footer.");
        }

        byte[] iv =
            body[..12];

        int encryptedLength =
            body.Length - 12;

        if (encryptedLength < 16)
        {
            throw new IOException(
                "Encrypted body is too short.");
        }

        byte[] encrypted =
            body[12..];

        int ciphertextLength =
            encrypted.Length - 16;

        byte[] ciphertext =
            encrypted[..ciphertextLength];

        byte[] tag =
            encrypted[ciphertextLength..];

        byte[] aad =
            header[4..18];

        byte[] payload;

        try
        {
            payload =
                AesGcmDecrypt(
                    key,
                    iv,
                    ciphertext,
                    tag,
                    aad);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException(
                "AES-GCM authentication failed.",
                ex);
        }

        return new TuyaFrame
        {
            Sequence = sequence,
            Command = command,
            RawLength = length,
            Payload = payload
        };
    }

    private static byte[] AesGcmEncrypt(
        byte[] key,
        byte[] iv,
        byte[] plaintext,
        byte[] aad)
    {
        byte[] ciphertext =
            new byte[plaintext.Length];

        byte[] tag =
            new byte[16];

        using AesGcm aes =
            new AesGcm(
                key,
                16);

        aes.Encrypt(
            iv,
            plaintext,
            ciphertext,
            tag,
            aad);

        byte[] result =
            new byte[
                ciphertext.Length +
                tag.Length];

        Buffer.BlockCopy(
            ciphertext,
            0,
            result,
            0,
            ciphertext.Length);

        Buffer.BlockCopy(
            tag,
            0,
            result,
            ciphertext.Length,
            tag.Length);

        return result;
    }

    private static byte[] AesGcmDecrypt(
        byte[] key,
        byte[] iv,
        byte[] ciphertext,
        byte[] tag,
        byte[] aad)
    {
        byte[] plaintext =
            new byte[ciphertext.Length];

        using AesGcm aes =
            new AesGcm(
                key,
                16);

        aes.Decrypt(
            iv,
            ciphertext,
            tag,
            plaintext,
            aad);

        return plaintext;
    }

    private static bool TryGetDouble(
        Dictionary<string, JsonElement> dps,
        string key,
        out double value)
    {
        value = 0;

        if (!dps.TryGetValue(
                key,
                out JsonElement element))
        {
            return false;
        }

        if (element.ValueKind ==
            JsonValueKind.Number)
        {
            return element.TryGetDouble(
                out value);
        }

        if (element.ValueKind ==
            JsonValueKind.String)
        {
            return double.TryParse(
                element.GetString(),
                out value);
        }

        return false;
    }

    private static async Task<byte[]> ReadExactAsync(
        NetworkStream stream,
        int length)
    {
        byte[] buffer =
            new byte[length];

        int offset = 0;

        while (offset < length)
        {
            int read =
                await stream.ReadAsync(
                    buffer,
                    offset,
                    length - offset);

            if (read == 0)
            {
                throw new IOException(
                    "The device closed the TCP connection.");
            }

            offset += read;
        }

        return buffer;
    }

    private static uint ReadUInt32BE(
        byte[] data,
        int offset)
    {
        return
            ((uint)data[offset] << 24) |
            ((uint)data[offset + 1] << 16) |
            ((uint)data[offset + 2] << 8) |
            data[offset + 3];
    }

    private static void WriteUInt32BE(
        byte[] data,
        int offset,
        uint value)
    {
        data[offset] =
            (byte)(value >> 24);

        data[offset + 1] =
            (byte)(value >> 16);

        data[offset + 2] =
            (byte)(value >> 8);

        data[offset + 3] =
            (byte)value;
    }

    private class TuyaFrame
    {
        public uint Sequence { get; set; }

        public uint Command { get; set; }

        public uint RawLength { get; set; }

        public byte[] Payload { get; set; } =
            Array.Empty<byte>();
    }
}