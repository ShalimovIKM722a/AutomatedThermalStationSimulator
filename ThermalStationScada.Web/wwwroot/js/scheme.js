function showReading(r) {
    document.querySelectorAll(
        `.sensor[data-topic="${r.topic}"]`
    ).forEach(el => {
        el.querySelector('.value').textContent =
            `${r.value} ${r.unit}`;

        const value = Number(r.value);
        const max = Number(el.dataset.max);

        el.classList.toggle('alarm', value > max);
    });
}

async function refresh() {
    try {
        const res = await fetch('/Scheme/Current');

        if (res.redirected) {
            window.location.reload();
            return;
        }

        if (!res.ok) {
            throw new Error(`HTTP ${res.status}`);
        }

        const list = await res.json();

        list.forEach(showReading);
    } catch (e) {
        console.error('Помилка оновлення датчиків', e);
    }
}

function showActuator(a) {
    document.querySelectorAll(
        `.actuator[data-topic="${a.topic}"]`
    ).forEach(el => {
        el.classList.toggle('on', a.isOn);
        el.classList.toggle('off', !a.isOn);
        el.querySelector('.state').textContent = a.isOn ? 'УВІМК' : 'ВИМК';
        el.title = a.reason;
    });
}

async function refreshActuators() {
    try {
        const res = await fetch('/Scheme/Actuators');

        if (res.redirected) {
            window.location.reload();
            return;
        }

        if (!res.ok) {
            throw new Error(`HTTP ${res.status}`);
        }

        const list = await res.json();

        list.forEach(showActuator);
    } catch (e) {
        console.error('Помилка оновлення механізмів', e);
    }
}

refresh();
refreshActuators();
setInterval(refresh, 2000);
setInterval(refreshActuators, 1000);