// 1. Grid View, Data View
document.querySelectorAll('.view-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        document.querySelectorAll('.view-btn').forEach(b => b.classList.remove('active'));
        this.classList.add('active');

        const view = this.dataset.view;
        if (view === 'card') {
            document.getElementById('cardView').style.display = 'grid';
            document.getElementById('tableView').style.display = 'none';
        } else {
            document.getElementById('cardView').style.display = 'none';
            document.getElementById('tableView').style.display = 'block';
        }
    });
});

// 2. Chi tiết thanh toán - Modal functions
function openTollModal(paymentId, code, type, licensePlate, owner, amount) {
    document.getElementById('modalInspectionIdInput').value = paymentId;
    document.getElementById('modalPaymentIdInput').value = paymentId;
    document.getElementById('modalInspectionCode').textContent = code;
    document.getElementById('modalInspectionType').textContent = type || 'N/A';
    document.getElementById('modalLicensePlate').textContent = licensePlate || 'N/A';
    document.getElementById('modalOwner').textContent = owner || 'N/A';
    document.getElementById('modalAmount').textContent = amount.toLocaleString('vi-VN') + ' VNĐ';
    document.getElementById('tollModal').classList.add('active');
}

function closeTollModal() {
    document.getElementById('tollModal').classList.remove('active');
}

// 3. Auto dismiss alerts after 5 seconds
setTimeout(function () {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        const closeBtn = alert.querySelector('.btn-close');
        if (closeBtn) {
            closeBtn.click();
        }
    });
}, 5000);

// 4. Chặn submit và điều hướng thanh toán PayOS
async function payWithPayOS(paymentId) {
    const res = await fetch(`/api/payos/create-link/${paymentId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    });

    if (!res.ok) {
        const text = await res.text();
        alert("Không tạo được link PayOS: " + text);
        return;
    }

    const data = await res.json();
    if (!data.checkoutUrl) {
        alert("Server không trả checkoutUrl.");
        return;
    }

    window.location.href = data.checkoutUrl;
}

document.getElementById('paymentForm').addEventListener('submit', async function (e) {
    const method = document.querySelector('input[name="paymentMethod"]:checked')?.value;

    if (method === "Chuyển khoản") {
        e.preventDefault(); // chặn submit CollectPayment

        const paymentId = document.getElementById('modalPaymentIdInput').value;
        if (!paymentId) {
            alert("Thiếu paymentId");
            return;
        }

        await payWithPayOS(paymentId);
    }
    // Nếu không phải "Chuyển khoản" thì để form submit bình thường (CollectPayment)
});

// 5. Details Modal
function openDetailsModal(code, type, plateNo, owner, phone, amount, paymentMethod, paidAt, createdAt) {
    document.getElementById('detailInspectionCode').textContent = code;
    document.getElementById('detailInspectionType').textContent = type;
    document.getElementById('detailCreatedAt').textContent = createdAt || 'N/A';
    document.getElementById('detailPlateNo').textContent = plateNo || 'N/A';
    document.getElementById('detailOwner').textContent = owner || 'N/A';
    document.getElementById('detailPhone').textContent = phone || 'N/A';
    document.getElementById('detailAmount').textContent = amount.toLocaleString('vi-VN') + ' VNĐ';
    document.getElementById('detailPaymentMethod').textContent = paymentMethod || 'N/A';
    document.getElementById('detailPaidAt').textContent = paidAt || 'Chưa thanh toán';

    // Timeline
    document.getElementById('timelineCreatedAt').textContent = createdAt || 'N/A';

    if (paidAt && paidAt !== '') {
        document.getElementById('timelinePaid').style.display = 'flex';
        document.getElementById('timelinePaidAt').textContent = paidAt;
    } else {
        document.getElementById('timelinePaid').style.display = 'none';
    }

    document.getElementById('detailsModal').classList.add('active');
}

function closeDetailsModal() {
    document.getElementById('detailsModal').classList.remove('active');
}

// 6. Print Receipt
function printReceipt(paymentId) {
    if (!paymentId) {
        alert("Thiếu payment ID.");
        return;
    }

    const url = `/receipt/printbypaymentid?paymentId=${paymentId}`;
    const w = window.open(url, '_blank', 'width=1200,height=800');

    if (!w) {
        alert("Trình duyệt chặn popup. Hãy cho phép popup để in biên nhận.");
        return;
    }

    w.onload = () => {
        try { w.print(); } catch { }
    };
}