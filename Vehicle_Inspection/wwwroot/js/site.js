function showFullScreenLoading(message = 'Đang tải...') {
    $('#fullscreen-loading').remove();

    $('body').append(`
        <div id="fullscreen-loading" class="vic-loading">
            <div class="vic-loading-card">
                <div class="vic-road">
                    <div class="vic-road-line"></div>
                    <i class="fa-solid fa-car-side vic-car"></i>
                    <span class="vic-smoke s1"></span>
                    <span class="vic-smoke s2"></span>
                    <span class="vic-smoke s3"></span>
                </div>

                <div class="vic-text">${message}</div>
            </div>
        </div>

        <style>
            .vic-loading{
                position: fixed;
                inset: 0;
                z-index: 9999;
                display: flex;
                align-items: center;
                justify-content: center;
                background: rgba(255,255,255,.55);
                backdrop-filter: blur(4px);
            }

            .vic-loading-card{
                width: min(520px, 92vw);
                padding: 22px 22px 18px;
                border-radius: 18px;
                background: rgba(255,255,255,.78);
                border: 1px solid rgba(15,23,42,.08);
                box-shadow: 0 18px 55px rgba(15,23,42,.18);
                text-align: center;
            }

            .vic-road{
                position: relative;
                height: 64px;
                border-radius: 14px;
                background: linear-gradient(180deg, rgba(15,23,42,.04), rgba(15,23,42,.02));
                overflow: hidden;
                margin-bottom: 12px;
            }

            /* vạch đường chạy */
            .vic-road-line{
                position: absolute;
                left: 0;
                top: 50%;
                width: 200%;
                height: 2px;
                transform: translateY(18px);
                background: repeating-linear-gradient(
                    90deg,
                    rgba(59,130,246,.0) 0 14px,
                    rgba(59,130,246,.45) 14px 34px
                );
               
                opacity: .9;
            }

            /* xe */
            .vic-car{
                position: absolute;
                top: 50%;
                transform: translateY(-12px);
                font-size: 32px;
                color: #2563eb;
                filter: drop-shadow(0 10px 18px rgba(37,99,235,.22));
                animation: vicDrive 4s cubic-bezier(.2,.8,.2,1) infinite;
            }

            /* khói nhẹ */
            .vic-smoke{
                position: absolute;
                top: 50%;
                width: 10px;
                height: 10px;
                border-radius: 999px;
                background: rgba(37,99,235,.18);
                transform: translateY(10px);
                opacity: 0;
            }

            .vic-smoke.s1{ animation: vicSmoke 1.1s infinite; }
            .vic-smoke.s2{ animation: vicSmoke 1.1s infinite .12s; }
            .vic-smoke.s3{ animation: vicSmoke 1.1s infinite .24s; }

            .vic-text{
                font-size: 16px;
                font-weight: 800;
                color: rgba(15,23,42,.78);
                letter-spacing: .2px;
            }

            @keyframes vicDrive{
                0%   { left: -15%; transform: translateY(-12px) rotate(0deg); }
                40%  { transform: translateY(-14px) rotate(-1deg); }
                60%  { transform: translateY(-10px) rotate(1deg); }
                100% { left: 110%; transform: translateY(-12px) rotate(0deg); }
            }

            @keyframes vicLine{
                0%   { transform: translate(-0%, 18px); }
                100% { transform: translate(-50%, 18px); }
            }

            @keyframes vicSmoke{
                0%{
                    opacity: 0;
                    transform: translateY(10px) translateX(0) scale(.8);
                }
                20%{ opacity: .9; }
                100%{
                    opacity: 0;
                    transform: translateY(10px) translateX(-40px) scale(1.6);
                }
            }
        </style>
    `);
}

function hideFullScreenLoading() {
    $('#fullscreen-loading').fadeOut(250, function () {
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
