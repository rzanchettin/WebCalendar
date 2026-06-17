
let selectedDay = null;
let selectedMonth = null;
let selectedYear = null;

// Detectar clique nas células do calendário
document.querySelectorAll('.clickable-day').forEach(cell => {
    cell.style.cursor = 'pointer';
    cell.addEventListener('click', function () {
        selectedDay = parseInt(this.getAttribute('data-day'));
        selectedMonth = parseInt(this.getAttribute('data-month'));
        selectedYear = parseInt(this.getAttribute('data-year'));

        const eventYear = document.getElementById('eventYear');
        const monthNames = ['', 'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
        document.getElementById('selectedDate').textContent = `${selectedDay} ${monthNames[selectedMonth]} ${selectedYear}`;
        document.getElementById('dateInfo').style.display = 'block';

        // Limpar formulário
        document.getElementById('eventForm').reset();
        document.getElementById('charCount').textContent = '0';
        eventYear.value = selectedYear;

        // Verificar se já existe aniversário/feriado/nota para este dia
        const existingBirthday = birthdays.find(b => b.day === selectedDay && b.month === selectedMonth);
        const existingHoliday = holidays.find(h => h.day === selectedDay && h.month === selectedMonth);
        const existingNotes = notes.find(n => n.day === selectedDay && n.month === selectedMonth);

        if (existingBirthday) {
            document.getElementById('eventType').value = 'birthdays';
            document.getElementById('eventName').value = decodeHtml(existingBirthday.name);
            document.getElementById('charCount').textContent = existingBirthday.name.length;
            document.getElementById('recurringSection').style.display = "none";
            document.getElementById('isRecurring').checked = true; // Aniversários são sempre recorrentes
        } else if (existingHoliday) {
            document.getElementById('eventType').value = 'holidays';
            document.getElementById('eventName').value = decodeHtml(existingHoliday.name);
            document.getElementById('charCount').textContent = existingHoliday.name.length;
            document.getElementById('recurringSection').style.display = "block";
            document.getElementById('isRecurring').checked = existingHoliday.recurring;

        } else if (existingNotes) {
            document.getElementById('eventType').value = 'notes';
            document.getElementById('eventName').value = decodeHtml(existingNotes.name);
            document.getElementById('charCount').textContent = existingNotes.name.length;
            document.getElementById('recurringSection').style.display = "block";
            document.getElementById('isRecurring').checked = existingNotes.recurring;
        }

        // Abrir modal
        const modal = new bootstrap.Modal(document.getElementById('addEventModal'));
        modal.show();
    });
});

// post do formulário ao alterar o ano para recarregar o calendário
document.getElementById('yearInput').addEventListener('change', function () {
    document.getElementById('mainForm').submit();
});


// Recorrente
document.getElementById('eventType').addEventListener('change', function () {
    document.getElementById('recurringSection').style.display = this.value === 'birthdays' ? 'none' : 'block';
});

// Contador de caracteres
document.getElementById('eventName').addEventListener('input', function () {
    document.getElementById('charCount').textContent = this.value.length;
});

// Botão Limpar
document.getElementById('btnClear').addEventListener('click', function () {
    document.getElementById('eventName').value = '';
    document.getElementById('isRecurring').checked = false;
    document.getElementById('charCount').textContent = '0';
});

// Botão Salvar
document.getElementById('btnSave').addEventListener('click', async function () {

    const eventType = document.getElementById('eventType').value.trim();
    const eventName = document.getElementById('eventName').value.trim();
    const eventYear = document.getElementById('eventYear').value;
    const eventRecurring = document.getElementById('isRecurring').checked;

    if (eventType == '') {
        alert('Por favor, selecione o tipo de evento.');
        return;
    }

    try {
        const payload = {
            day: selectedDay,
            month: selectedMonth,
            type: eventType,
            name: eventName,
            year: eventYear,
            recurring: eventRecurring
        };

        console.log('Enviando:', payload);

        const response = await fetch('/api/event/save', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        console.log('Status:', response.status);

        const contentType = response.headers.get('content-type');
        console.log('Content-Type:', contentType);

        let responseData = null;

        if (contentType && contentType.includes('application/json')) {
            responseData = await response.json();
        } else {
            const text = await response.text();
            console.log('Resposta (texto):', text);
            responseData = { message: text || 'Erro desconhecido' };
        }

        console.log('Resposta (parsed):', responseData);

        if (response.ok) {
            // Fechar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addEventModal'));
            modal.hide();
            // Recarregar página para atualizar calendário
            location.reload();
        } else {
            alert('Erro ao salvar evento: ' + (responseData.message || responseData));
        }
    } catch (error) {
        console.error('Erro:', error);
        alert('Erro ao salvar evento: ' + error.message);
    }
});

function decodeHtml(html) {
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
}

