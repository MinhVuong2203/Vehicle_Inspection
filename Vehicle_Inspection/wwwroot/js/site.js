// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Thêm vào file site.js hoặc _Layout.cshtml

// Function loading toàn màn hình
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
            @keyframes spin {
                to { transform: rotate(360deg); }
            }
        </style>
    `);
}

function hideFullScreenLoading() {
    $('#fullscreen-loading').fadeOut(300, function () {
        $(this).remove();
    });
}

// ========== AJAX NAVIGATION ==========
$(function () {
    // Xử lý click navigation
    $('.admin-subnav .nav').on('click', '.nav-link', function (e) {
        e.preventDefault();

        var href = $(this).attr('href');
        if (!href || href === '#') {
            return;
        }
        // Remove active from other links and add to clicked one
        $('.admin-subnav .nav .nav-link').removeClass('active');
        $(this).addClass('active');

        // Save active tab to localStorage
        localStorage.setItem('activeNavHref', href);

        // Load content via AJAX
        loadPageContent(href);
    });

    // Restore active state on page load
    var savedHref = localStorage.getItem('activeNavHref');
    if (savedHref) {
        $('.admin-subnav .nav .nav-link').removeClass('active');
        $('.admin-subnav .nav .nav-link[href="' + savedHref + '"]').addClass('active');
    } else {
        // Set active based on current URL
        var currentPath = window.location.pathname;
        $('.admin-subnav .nav .nav-link').each(function () {
            if ($(this).attr('href') === currentPath) {
                $(this).addClass('active');
            }
        });
    }

    // ========== AUTO LOADING CHO TẤT CẢ FORM ==========
    // Bắt sự kiện submit cho tất cả form (trừ form logout)
    $(document).on('submit', 'form', function (e) {
        var $form = $(this);

        // Bỏ qua form logout hoặc form có class 'no-loading'
        if ($form.attr('asp-action') === 'Logout' ||
            $form.hasClass('no-loading') ||
            $form.data('ajax') === false) {
            return; // Để form submit bình thường
        }

        // Nếu form đã có xử lý AJAX riêng thì bỏ qua
        if ($form.data('ajax-handled')) {
            return;
        }

        e.preventDefault();
        showFullScreenLoading('Đang xử lý...');

        $.ajax({
            url: $form.attr('action'),
            type: $form.attr('method') || 'POST',
            data: $form.serialize(),
            success: function (response) {
                hideFullScreenLoading();

                // Nếu response là HTML, update main content
                if (typeof response === 'string' && response.includes('<')) {
                    var content = $(response).find('main[role="main"]').html();
                    if (content) {
                        $('main[role="main"]').html(content);
                    } else {
                        $('main[role="main"]').html(response);
                    }
                } else if (response.success) {
                    // Nếu là JSON response
                    if (response.redirectUrl) {
                        window.location.href = response.redirectUrl;
                    } else if (response.message) {
                        alert(response.message);
                    }
                }
            },
            error: function (xhr) {
                hideFullScreenLoading();

                if (xhr.responseJSON && xhr.responseJSON.message) {
                    alert('Lỗi: ' + xhr.responseJSON.message);
                } else {
                    alert('Có lỗi xảy ra khi xử lý. Vui lòng thử lại!');
                }
            }
        });
    });

    // ========== AUTO LOADING CHO CÁC LINK (TÙY CHỌN) ==========
    // Nếu muốn loading khi click vào link thường
    $(document).on('click', 'a[data-ajax="true"]', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');

        if (url && url !== '#') {
            loadPageContent(url);
        }
    });
});

// Load page content function
function loadPageContent(url) {
    showFullScreenLoading('Đang tải dữ liệu...');

    $.ajax({
        url: url,
        type: 'GET',
        success: function (response) {
            var content = $(response).find('main[role="main"]').html();
            if (content) {
                $('main[role="main"]').html(content);
            } else {
                $('main[role="main"]').html(response);
            }

            window.history.pushState({ path: url }, '', url);
            hideFullScreenLoading();
        },
        error: function (xhr, status, error) {
            hideFullScreenLoading();
            $('main[role="main"]').html('<div class="alert alert-danger">Có lỗi xảy ra khi tải trang. Vui lòng thử lại.</div>');
            console.error('Error loading page:', error);
        }
    });
}

// Handle browser back/forward buttons
window.addEventListener('popstate', function (e) {
    if (e.state && e.state.path) {
        loadPageContent(e.state.path);
    }
});