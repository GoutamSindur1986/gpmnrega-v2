/**
 * chatbot.js
 * Chatbot UI scaffold. UI is fully built and works.
 * Backend methods are stubs — fill in when ready.
 * WebRTC call button is imported as a placeholder.
 */

(function() {
    'use strict';

    // ── Config ─────────────────────────────────────────────────────
    var CHAT_CONFIG = {
        botName: 'GP Assistant',
        greeting: 'Hi! I\'m your GP MNREGA assistant. How can I help you today?',
        quickReplies: [
            'How do I generate a Form 6?',
            'What is NMR?',
            'How to check FTO status?',
            'Subscription plans'
        ]
    };

    // ── State ──────────────────────────────────────────────────────
    var isOpen = false;
    var isTyping = false;
    var messageHistory = [];

    // ── Inject chatbot HTML into page ──────────────────────────────
    function init() {
        if (document.getElementById('gpChatbot')) return;

        document.body.insertAdjacentHTML('beforeend', getChatbotHtml());
        bindEvents();
        showGreeting();
    }

    function getChatbotHtml() {
        return `
        <style>
        .gp-chat-btn {
            position: fixed; bottom: 28px; right: 28px; z-index: 9000;
            width: 56px; height: 56px; border-radius: 50%;
            background: linear-gradient(135deg, #3730a3, #4f46e5);
            border: none; cursor: pointer; color: #fff;
            box-shadow: 0 8px 32px rgba(55,48,163,.4);
            display: flex; align-items: center; justify-content: center;
            font-size: 1.4rem; transition: all .2s;
        }
        .gp-chat-btn:hover { transform: scale(1.08); }
        .gp-chat-btn .chat-badge {
            position: absolute; top: -4px; right: -4px;
            width: 18px; height: 18px; background: #ef4444;
            border-radius: 50%; font-size: .65rem; font-weight: 700;
            display: flex; align-items: center; justify-content: center;
            color: #fff; border: 2px solid #fff;
        }

        .gp-chat-window {
            position: fixed; bottom: 96px; right: 28px; z-index: 8999;
            width: 360px; max-height: 520px;
            background: #fff; border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,.15);
            display: flex; flex-direction: column;
            transform: translateY(20px) scale(.95); opacity: 0;
            pointer-events: none; transition: all .25s ease;
            overflow: hidden;
        }
        .gp-chat-window.open {
            transform: translateY(0) scale(1); opacity: 1;
            pointer-events: all;
        }
        @media(max-width:480px) {
            .gp-chat-window { width: calc(100vw - 32px); right: 16px; bottom: 88px; }
        }

        .chat-header {
            background: linear-gradient(135deg, #3730a3, #4f46e5);
            color: #fff; padding: 14px 16px;
            display: flex; align-items: center; gap: 10px;
            flex-shrink: 0;
        }
        .chat-header .avatar {
            width: 36px; height: 36px; border-radius: 50%;
            background: rgba(255,255,255,.2);
            display: flex; align-items: center; justify-content: center;
            font-size: 1rem;
        }
        .chat-header .info { flex: 1; }
        .chat-header .name { font-weight: 600; font-size: .9rem; }
        .chat-header .status {
            font-size: .72rem; opacity: .8;
            display: flex; align-items: center; gap: 4px;
        }
        .chat-header .status::before {
            content: ''; width: 7px; height: 7px;
            background: #4ade80; border-radius: 50%;
        }
        .chat-header .call-btn {
            background: rgba(255,255,255,.15); border: none; color: #fff;
            width: 32px; height: 32px; border-radius: 50%;
            cursor: pointer; display: flex; align-items: center;
            justify-content: center; font-size: .9rem; transition: all .15s;
            title: 'Voice call (coming soon)';
        }
        .chat-header .call-btn:hover { background: rgba(255,255,255,.25); }
        .chat-header .close-btn {
            background: none; border: none; color: rgba(255,255,255,.8);
            font-size: 1.1rem; cursor: pointer; padding: 0;
        }

        .chat-messages {
            flex: 1; overflow-y: auto; padding: 16px;
            display: flex; flex-direction: column; gap: 10px;
            scrollbar-width: thin;
        }
        .chat-msg { display: flex; align-items: flex-end; gap: 8px; }
        .chat-msg.bot { justify-content: flex-start; }
        .chat-msg.user { justify-content: flex-end; }
        .chat-msg .bubble {
            max-width: 80%; padding: 10px 14px;
            border-radius: 16px; font-size: .85rem; line-height: 1.5;
        }
        .chat-msg.bot .bubble {
            background: #f1f5f9; color: #1e293b;
            border-bottom-left-radius: 4px;
        }
        .chat-msg.user .bubble {
            background: linear-gradient(135deg, #3730a3, #4f46e5);
            color: #fff; border-bottom-right-radius: 4px;
        }
        .chat-msg .bot-avatar {
            width: 28px; height: 28px; border-radius: 50%;
            background: #ede9fe; display: flex; align-items: center;
            justify-content: center; font-size: .8rem; flex-shrink: 0;
        }
        .typing-indicator span {
            display: inline-block; width: 7px; height: 7px;
            border-radius: 50%; background: #94a3b8; margin: 0 2px;
            animation: bounce 1.2s infinite;
        }
        .typing-indicator span:nth-child(2) { animation-delay: .2s; }
        .typing-indicator span:nth-child(3) { animation-delay: .4s; }
        @keyframes bounce {
            0%,60%,100% { transform: translateY(0); }
            30% { transform: translateY(-6px); }
        }

        .quick-replies {
            padding: 8px 16px; display: flex; flex-wrap: wrap; gap: 6px;
        }
        .quick-reply {
            background: #f1f5f9; border: 1.5px solid #e2e8f0;
            border-radius: 20px; padding: 5px 12px;
            font-size: .78rem; cursor: pointer; transition: all .15s;
            white-space: nowrap;
        }
        .quick-reply:hover { background: #ede9fe; border-color: #a5b4fc; color: #3730a3; }

        .chat-input-row {
            padding: 12px 14px; border-top: 1px solid #f1f5f9;
            display: flex; gap: 8px; flex-shrink: 0;
        }
        .chat-input-row input {
            flex: 1; border: 1.5px solid #e2e8f0; border-radius: 24px;
            padding: 9px 16px; font-size: .85rem; font-family: 'Poppins', sans-serif;
            outline: none; transition: all .2s;
        }
        .chat-input-row input:focus { border-color: #a5b4fc; }
        .chat-send-btn {
            width: 38px; height: 38px; border-radius: 50%;
            background: linear-gradient(135deg, #3730a3, #4f46e5);
            border: none; color: #fff; cursor: pointer;
            display: flex; align-items: center; justify-content: center;
            font-size: .9rem; flex-shrink: 0; transition: all .2s;
        }
        .chat-send-btn:hover { filter: brightness(1.1); }
        </style>

        <div id="gpChatbot">
            <!-- Floating button -->
            <button class="gp-chat-btn" id="chatToggleBtn" onclick="gpChatToggle()"
                    aria-label="Open support chat">
                <i class="bi bi-chat-dots-fill" id="chatBtnIcon"></i>
                <span class="chat-badge" id="chatBadge" style="display:none">1</span>
            </button>

            <!-- Chat window -->
            <div class="gp-chat-window" id="chatWindow" role="dialog"
                 aria-label="GP MNREGA Support Chat">
                <div class="chat-header">
                    <div class="avatar"><i class="bi bi-robot"></i></div>
                    <div class="info">
                        <div class="name">${CHAT_CONFIG.botName}</div>
                        <div class="status">Online</div>
                    </div>
                    <!-- WebRTC call button — placeholder, wired up in Phase 2 -->
                    <button class="call-btn" onclick="gpChatStartCall()" title="Voice call (coming soon)">
                        <i class="bi bi-telephone-fill"></i>
                    </button>
                    <button class="close-btn" onclick="gpChatToggle()"
                            aria-label="Close chat"><i class="bi bi-x-lg"></i></button>
                </div>

                <div class="chat-messages" id="chatMessages"></div>

                <div class="quick-replies" id="quickReplies">
                    ${CHAT_CONFIG.quickReplies.map(q =>
                        `<button class="quick-reply" onclick="gpChatSendQuick('${q.replace(/'/g, "\\'")}')">${q}</button>`
                    ).join('')}
                </div>

                <div class="chat-input-row">
                    <input type="text" id="chatInput" placeholder="Type a message…"
                           onkeydown="if(event.key==='Enter') gpChatSend()"/>
                    <button class="chat-send-btn" onclick="gpChatSend()"
                            aria-label="Send message">
                        <i class="bi bi-send-fill"></i>
                    </button>
                </div>
            </div>
        </div>`;
    }

    function bindEvents() {}

    function showGreeting() {
        setTimeout(function() {
            addBotMessage(CHAT_CONFIG.greeting);
            document.getElementById('chatBadge').style.display = 'flex';
        }, 1500);
    }

    // ── Public API ─────────────────────────────────────────────────
    window.gpChatToggle = function() {
        isOpen = !isOpen;
        var win = document.getElementById('chatWindow');
        var icon = document.getElementById('chatBtnIcon');
        var badge = document.getElementById('chatBadge');
        win.classList.toggle('open', isOpen);
        icon.className = isOpen ? 'bi bi-x-lg' : 'bi bi-chat-dots-fill';
        if (isOpen) badge.style.display = 'none';
    };

    window.gpChatSend = function() {
        var input = document.getElementById('chatInput');
        var msg = input.value.trim();
        if (!msg) return;
        input.value = '';
        sendMessage(msg);
    };

    window.gpChatSendQuick = function(msg) {
        document.getElementById('quickReplies').style.display = 'none';
        sendMessage(msg);
    };

    // ── WebRTC call placeholder ────────────────────────────────────
    // Full implementation will come in Phase 2 (WebRTC session)
    window.gpChatStartCall = function() {
        addBotMessage('Voice calls are coming soon! We\'re setting up our WebRTC infrastructure. Stay tuned! 🔔');
    };

    // ── Message handling ───────────────────────────────────────────
    function sendMessage(text) {
        addUserMessage(text);
        messageHistory.push({ role: 'user', content: text });
        showTyping();
        getBotResponse(text).then(function(response) {
            hideTyping();
            addBotMessage(response);
            messageHistory.push({ role: 'assistant', content: response });
        });
    }

    function addBotMessage(text) {
        var el = document.createElement('div');
        el.className = 'chat-msg bot';
        el.innerHTML = '<div class="bot-avatar"><i class="bi bi-robot"></i></div>' +
            '<div class="bubble">' + escapeHtml(text) + '</div>';
        appendMessage(el);
    }

    function addUserMessage(text) {
        var el = document.createElement('div');
        el.className = 'chat-msg user';
        el.innerHTML = '<div class="bubble">' + escapeHtml(text) + '</div>';
        appendMessage(el);
    }

    function showTyping() {
        var el = document.createElement('div');
        el.className = 'chat-msg bot'; el.id = 'typingIndicator';
        el.innerHTML = '<div class="bot-avatar"><i class="bi bi-robot"></i></div>' +
            '<div class="bubble typing-indicator"><span></span><span></span><span></span></div>';
        appendMessage(el);
        isTyping = true;
    }

    function hideTyping() {
        var el = document.getElementById('typingIndicator');
        if (el) el.remove();
        isTyping = false;
    }

    function appendMessage(el) {
        var container = document.getElementById('chatMessages');
        container.appendChild(el);
        container.scrollTop = container.scrollHeight;
    }

    // ── Bot response logic ─────────────────────────────────────────
    // STUB: Replace with actual API call (Claude API, OpenAI, etc.)
    // when chatbot backend is implemented.
    function getBotResponse(userMessage) {
        return new Promise(function(resolve) {
            var delay = 800 + Math.random() * 600;
            setTimeout(function() {
                resolve(generateLocalResponse(userMessage));
            }, delay);
        });
    }

    // Basic keyword matching — replace with real AI when ready
    function generateLocalResponse(msg) {
        var m = msg.toLowerCase();

        if (m.includes('form 6') || m.includes('form6'))
            return 'Form 6 is the Job Card Application Register. To generate it: Search for a work code on the home page → click the NMR number → click "Form 6".';

        if (m.includes('nmr'))
            return 'NMR (National Muster Roll) records daily attendance of workers. You can download blank NMRs and filled NMRs from the Work Overview page.';

        if (m.includes('fto'))
            return 'FTO (Fund Transfer Order) is used to transfer wages to workers\' bank accounts. You can download FTO details from the NMR list on the work overview page.';

        if (m.includes('subscription') || m.includes('plan') || m.includes('paid'))
            return 'We offer subscription plans for unlimited report generation without watermarks. Visit the Subscription page for pricing details.';

        if (m.includes('cashbook') || m.includes('register'))
            return 'Go to Cashbooks & Registers from the top menu. Select the financial year and month, then click the register you want to download.';

        if (m.includes('forgot password') || m.includes('reset password'))
            return 'Click "Forgot password?" on the login page and enter your email. We\'ll send you a reset link.';

        if (m.includes('hello') || m.includes('hi') || m.includes('hey'))
            return 'Hello! How can I help you with GP MNREGA today?';

        // Default
        return 'I\'m not sure about that yet. Please contact our support at admin@gpmnrega.com or call 1800-111-222. Our team will be happy to help!';
    }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // ── Auto-init when DOM is ready ────────────────────────────────
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
