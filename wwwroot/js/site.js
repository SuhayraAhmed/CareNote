$(document).ready(function() {
    // Mobile menu toggle
    const mobileMenuBtn = $('#mobileMenuBtn');
    const sidebar = $('.sidebar');
    const overlay = $('<div class="sidebar-overlay"></div>');
    $('body').append(overlay);

    // Toggle sidebar on mobile
    mobileMenuBtn.on('click', function() {
        sidebar.toggleClass('open');
        overlay.toggleClass('active');
    });

    // Close sidebar when clicking overlay
    overlay.on('click', function() {
        sidebar.removeClass('open');
        overlay.removeClass('active');
    });

    // Close sidebar when clicking a nav link on mobile
    $('.nav-item').on('click', function() {
        if ($(window).width() <= 1024) {
            sidebar.removeClass('open');
            overlay.removeClass('active');
        }
    });

    // Handle window resize
    $(window).on('resize', function() {
        if ($(window).width() > 1024) {
            sidebar.removeClass('open');
            overlay.removeClass('active');
        }
    });

    // Auto-resize textareas
    $('textarea').on('input', function() {
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    });

    // Filter buttons
    $('.filter-btn').on('click', function() {
        $('.filter-btn').removeClass('active');
        $(this).addClass('active');

        const filter = $(this).data('filter');
        filterDocuments(filter);
    });

    // Search functionality
    $('#searchInput').on('input', function() {
        const searchTerm = $(this).val().toLowerCase();
        filterDocuments(null, searchTerm);
    });

    function filterDocuments(filter, searchTerm) {
        $('.document-card').each(function() {
            const card = $(this);
            const title = card.find('.document-title').text().toLowerCase();
            const preview = card.find('.document-preview').text().toLowerCase();
            const format = card.find('.badge').text().toLowerCase();

            let matchesFilter = true;
            let matchesSearch = true;

            if (filter && filter !== 'all') {
                matchesFilter = format.includes(filter);
            }

            if (searchTerm) {
                matchesSearch = title.includes(searchTerm) || preview.includes(searchTerm);
            }

            if (matchesFilter && matchesSearch) {
                card.fadeIn(200);
            } else {
                card.fadeOut(200);
            }
        });
    }

    // Favorite button toggle
    $('.favorite-btn').on('click', function(e) {
        e.preventDefault();
        $(this).toggleClass('active');
        const icon = $(this).find('i');
        if ($(this).hasClass('active')) {
            icon.removeClass('far').addClass('fas');
        } else {
            icon.removeClass('fas').addClass('far');
        }
    });

    // Copy to clipboard functionality
    window.copyToClipboard = function(text) {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand('copy');
        document.body.removeChild(textarea);

        // Show success message
        showNotification('Copied to clipboard!', 'success');
    };

    // Notification function
    window.showNotification = function(message, type = 'info') {
        const notification = $(`
            <div class="notification notification-${type} fade-in">
                <i class="fas fa-check-circle"></i>
                <span>${message}</span>
            </div>
        `);

        $('body').append(notification);

        setTimeout(() => {
            notification.fadeOut(300, function() {
                $(this).remove();
            });
        }, 3000);
    };

    // Initialize fade-in animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    $('.fade-in').each(function() {
        observer.observe(this);
    });
});

// Add this CSS for notifications to your site.css or add inline
const notificationStyles = `
<style>
.notification {
    position: fixed;
    top: 20px;
    right: 20px;
    background: white;
    padding: 1rem 1.5rem;
    border-radius: 12px;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
    display: flex;
    align-items: center;
    gap: 0.75rem;
    z-index: 9999;
    animation: slideIn 0.3s ease;
}

.notification-success {
    border-left: 4px solid #10B981;
}

.notification-success i {
    color: #10B981;
    font-size: 1.25rem;
}

.sidebar-overlay {
    display: none;
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 35;
    opacity: 0;
    transition: opacity 0.3s ease;
}

.sidebar-overlay.active {
    display: block;
    opacity: 1;
}

@keyframes slideIn {
    from {
        transform: translateX(100%);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}
</style>
`;

$(document).ready(function() {
    $('head').append(notificationStyles);
});