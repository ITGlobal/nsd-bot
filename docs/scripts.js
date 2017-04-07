$(function() {
    var chatIcon = $('.chat-icon');
    var chatWindow = $('.chat-window');
    var chatWindowClose = $('.chat-window-close', chatWindow);

    chatIcon.delay(500).fadeIn();

    chatIcon.on('click', function() {
        chatIcon.hide();
        chatWindow.slideDown();
    });

    chatWindowClose.on('click', function() {
        chatWindow.slideUp(function() {
            chatIcon.fadeIn();
        });
    });
});
