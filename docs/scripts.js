$(() => {
    var chatIcon = $('.chat-icon');
    var chatWindow = $('.chat-window');
    var chatWindowClose = $('.chat-window-close', chatWindow);

    chatIcon.delay(2000).fadeIn();

    chatIcon.on('click', () => {
        chatIcon.hide();
        chatWindow.slideDown();
    });

    chatWindowClose.on('click', () => {
        chatWindow.slideUp(() => {
            chatIcon.fadeIn();
        });
    });
});
