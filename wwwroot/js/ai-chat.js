class AIChat {
    constructor() {
        this.chatMessages = document.getElementById('chatMessages');
        this.messageInput = document.getElementById('messageInput');
        this.sendButton = document.getElementById('sendButton');
        this.isLoading = false;

        this.init();
    }

    init() {
        this.sendButton.addEventListener('click', () => this.sendMessage());
        this.messageInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        });

        // Lägg till snabbåtgärder
        this.addQuickActionButtons();

        // Exempel prompts
        document.querySelectorAll('.example-prompt').forEach(button => {
            button.addEventListener('click', () => {
                this.messageInput.value = button.getAttribute('data-question');
                this.sendMessage();
            });
        });

        this.autoResizeTextarea();
    }

    addQuickActionButtons() {
        const quickActions = document.createElement('div');
        quickActions.className = 'quick-action-buttons';
        quickActions.innerHTML = `
            <div class="quick-action-header">Snabbformuleringar:</div>
            <button class="quick-action-btn" data-text="förbättra: patienten är arg">
                 Förbättra "patienten är arg"
            </button>
            <button class="quick-action-btn" data-text="förbättra: sover dåligt">
                 Förbättra "sover dåligt"
            </button>
            <button class="quick-action-btn" data-text="förbättra: äter inte">
                 Förbättra "äter inte"
            </button>
            <button class="quick-action-btn" data-text="förbättra: orolig">
                 Förbättra "orolig"
            </button>
        `;

        // Lägg till snabbåtgärder ovanför chatten
        if (this.chatMessages && this.chatMessages.parentNode) {
            this.chatMessages.parentNode.insertBefore(quickActions, this.chatMessages);
        }

        // Lägg till event listeners för knapparna
        document.querySelectorAll('.quick-action-btn').forEach(button => {
            button.addEventListener('click', () => {
                if (this.messageInput) {
                    this.messageInput.value = button.getAttribute('data-text');
                    this.sendMessage();
                }
            });
        });
    }

    autoResizeTextarea() {
        if (this.messageInput) {
            this.messageInput.addEventListener('input', () => {
                this.messageInput.style.height = 'auto';
                this.messageInput.style.height = Math.min(this.messageInput.scrollHeight, 120) + 'px';
            });
        }
    }

    async sendMessage() {
        const message = this.messageInput.value.trim();
        if (!message || this.isLoading) return;

        this.addMessage(message, 'user');
        this.messageInput.value = '';
        if (this.messageInput) {
            this.messageInput.style.height = 'auto';
        }

        this.showLoading();
        this.isLoading = true;
        this.sendButton.disabled = true;

        try {
            const response = await fetch('/AIChat/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ message: message })
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const data = await response.json();
            this.addMessage(data.reply, 'assistant');

        } catch (error) {
            console.error('Error:', error);
            this.addMessage('Sorry, I encountered an error. Please try again.', 'assistant', true);
        } finally {
            this.hideLoading();
            this.isLoading = false;
            this.sendButton.disabled = false;
            if (this.messageInput) {
                this.messageInput.focus();
            }
        }
    }

    addMessage(text, sender, isError = false) {
        // Remove empty state if it exists
        const emptyState = this.chatMessages.querySelector('.empty-chat-state');
        if (emptyState) {
            emptyState.remove();
        }

        const messageDiv = document.createElement('div');
        messageDiv.className = `chat-message ${sender}-message ${isError ? 'error-message' : ''}`;

        const timestamp = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        messageDiv.innerHTML = `
            ${sender === 'assistant' ? '<div class="message-avatar"><i class="fas fa-robot"></i></div>' : ''}
            <div class="message-content">
                <div class="message-bubble">
                    ${this.formatMessage(text)}
                </div>
                <div class="message-time">${timestamp}</div>
            </div>
            ${sender === 'user' ? '<div class="message-avatar user-avatar"><i class="fas fa-user"></i></div>' : ''}
        `;

        if (this.chatMessages) {
            this.chatMessages.appendChild(messageDiv);
            this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
        }
    }

    formatMessage(text) {
        // Simple markdown-like formatting
        if (typeof text !== 'string') return '';

        return text
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\n/g, '<br>');
    }

    showLoading() {
        const loadingDiv = document.createElement('div');
        loadingDiv.className = 'chat-message assistant-message loading-message';
        loadingDiv.id = 'loadingMessage';
        loadingDiv.innerHTML = `
            <div class="message-avatar"><i class="fas fa-robot"></i></div>
            <div class="message-content">
                <div class="message-bubble">
                    <div class="typing-indicator">
                        <span></span>
                        <span></span>
                        <span></span>
                    </div>
                </div>
            </div>
        `;
        if (this.chatMessages) {
            this.chatMessages.appendChild(loadingDiv);
            this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
        }
    }

    hideLoading() {
        const loadingMessage = document.getElementById('loadingMessage');
        if (loadingMessage) {
            loadingMessage.remove();
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new AIChat();
});