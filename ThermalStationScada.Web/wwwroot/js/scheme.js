function showReading(r) {
    document.querySelectorAll(
        `.sensor[data-topic="${r.topic}"]`
    ).forEach(el => {
        el.querySelector('.value').textContent =
            `${r.value} ${r.unit}`;
    });
}

async function refresh() {
    try {
        const res = await fetch('/Scheme/Current');

        if (!res.ok) {
            throw new Error(`HTTP ${res.status}`);
        }

        const list = await res.json();

        console.log('Отримані дані:', list);

        list.forEach(showReading);
    } catch (e) {
        console.error('Помилка оновлення', e);
    }
}

refresh();
setInterval(refresh, 2000);