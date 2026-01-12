function showFullScreenLoading(message = 'Đang tải...') {
    $('#fullscreen-loading').remove();
    $('body').append(`
        <div id="fullscreen-loading" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.05); z-index: 9999; display: flex; align-items: center; justify-content: center;">
            <div style="text-align: center;">
                <div style="position: relative; width: 60px; height: 60px; margin: 0 auto;">
                    <div style="position: absolute; width: 100%; height: 100%; border: 5px solid rgba(40, 167, 69, 0.3); border-top-color: #28a745; border-radius: 50%; animation: spin 1s linear infinite;"></div>
                    <div style="position: absolute; width: 100%; height: 100%; border: 5px solid transparent; border-top-color: #20c997; border-radius: 50%; animation: spin 1.5s linear infinite; animation-direction: reverse;"></div>
                </div>
                <p class="mt-4" style="font-size: 18px; font-weight: 600; color: #28a745;">${message}</p>
            </div>
        </div>
        <style>
            @keyframes spin { to { transform: rotate(360deg); } }
        </style>
    `);
}

function hideFullScreenLoading() {
    $('#fullscreen-loading').fadeOut(300, function () {
        $(this).remove();
    });
}

$(function () {
    // Submit form: show loading nhưng vẫn submit bình thường
    $(document).on('submit', 'form', function () {
        const $form = $(this);

        // Bỏ qua logout hoặc form bạn không muốn loading
        if ($form.hasClass('no-loading')) return;

        showFullScreenLoading('Đang xử lý...');
    });

    // Click link điều hướng: show loading (tùy chọn)
    $(document).on('click', 'a', function () {
        const href = $(this).attr('href');
        if (!href || href === '#' || href.startsWith('javascript:')) return;
        if ($(this).attr('target') === '_blank') return;

        showFullScreenLoading('Đang tải...');
    });

    // Khi trang load xong thì ẩn loading
    $(window).on('load', function () {
        hideFullScreenLoading();
    });
});
