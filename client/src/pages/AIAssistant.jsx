import { useState } from "react";
import api from "../services/api";

function AIAssistant() {
    const [messages, setMessages] = useState([
        {
            role: "assistant",
            content:
                "Hello! I'm the AgencyHub AI Assistant. I can help you understand your agency workflow, projects, tasks, clients, and productivity."
        }
    ]);

    const [input, setInput] = useState("");
    const [loading, setLoading] = useState(false);

    const quickQuestions = [
        "Give me a summary of AgencyHub.",
        "How can I manage my projects?",
        "How should I organize pending tasks?",
        "What information should an agency dashboard show?"
    ];

    const sendMessage = async (messageOverride = null) => {
        const message = (messageOverride ?? input).trim();

        if (!message || loading) {
            return;
        }

        setMessages((previous) => [
            ...previous,
            {
                role: "user",
                content: message
            }
        ]);

        setInput("");
        setLoading(true);

        try {
            const response = await api.post("/ai/chat", {
                message
            });

            setMessages((previous) => [
                ...previous,
                {
                    role: "assistant",
                    content:
                        response.data?.message ||
                        "I couldn't generate a response."
                }
            ]);
        } catch (error) {
            const errorMessage =
                error.response?.data?.message ||
                "The AI assistant is currently unavailable.";

            setMessages((previous) => [
                ...previous,
                {
                    role: "assistant",
                    content: errorMessage,
                    error: true
                }
            ]);
        } finally {
            setLoading(false);
        }
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        await sendMessage();
    };

    const handleKeyDown = async (event) => {
        if (event.key === "Enter" && !event.shiftKey) {
            event.preventDefault();
            await sendMessage();
        }
    };

    const clearChat = () => {
        setMessages([
            {
                role: "assistant",
                content:
                    "Chat cleared. How can I help you with AgencyHub?"
            }
        ]);
    };

    return (
        <div className="ai-page">
            <div className="ai-header">
                <div>
                    <div className="ai-title-row">
                        <div className="ai-icon">🤖</div>

                        <div>
                            <h1>AI Assistant</h1>
                            <p>
                                Your intelligent AgencyHub productivity
                                assistant
                            </p>
                        </div>
                    </div>
                </div>

                <button
                    type="button"
                    className="ai-clear-button"
                    onClick={clearChat}
                >
                    Clear Chat
                </button>
            </div>

            <div className="ai-content">
                <div className="ai-chat-card">
                    <div className="ai-chat-header">
                        <div>
                            <strong>AgencyHub AI</strong>

                            <span className="ai-status">
                                <span className="ai-status-dot"></span>
                                Online
                            </span>
                        </div>
                    </div>

                    <div className="ai-messages">
                        {messages.map((message, index) => (
                            <div
                                key={`${message.role}-${index}`}
                                className={`ai-message-row ${message.role === "user"
                                        ? "user-message"
                                        : "assistant-message"
                                    }`}
                            >
                                {message.role === "assistant" && (
                                    <div className="message-avatar">
                                        🤖
                                    </div>
                                )}

                                <div
                                    className={`ai-message ${message.error
                                            ? "ai-error"
                                            : ""
                                        }`}
                                >
                                    {message.content}
                                </div>

                                {message.role === "user" && (
                                    <div className="message-avatar user-avatar">
                                        👤
                                    </div>
                                )}
                            </div>
                        ))}

                        {loading && (
                            <div className="ai-message-row assistant-message">
                                <div className="message-avatar">
                                    🤖
                                </div>

                                <div className="ai-message typing">
                                    <span></span>
                                    <span></span>
                                    <span></span>
                                </div>
                            </div>
                        )}
                    </div>

                    <div className="quick-questions">
                        <span>Quick questions</span>

                        <div className="quick-question-list">
                            {quickQuestions.map((question) => (
                                <button
                                    key={question}
                                    type="button"
                                    onClick={() =>
                                        sendMessage(question)
                                    }
                                    disabled={loading}
                                >
                                    {question}
                                </button>
                            ))}
                        </div>
                    </div>

                    <form
                        className="ai-input-area"
                        onSubmit={handleSubmit}
                    >
                        <textarea
                            value={input}
                            onChange={(event) =>
                                setInput(event.target.value)
                            }
                            onKeyDown={handleKeyDown}
                            placeholder="Ask AgencyHub AI anything..."
                            maxLength={4000}
                            rows={2}
                            disabled={loading}
                        />

                        <button
                            type="submit"
                            disabled={
                                loading ||
                                !input.trim()
                            }
                        >
                            {loading ? "..." : "Send"}
                        </button>
                    </form>

                    <div className="ai-disclaimer">
                        AI responses may not always be accurate. Verify
                        important information before taking action.
                    </div>
                </div>

                <div className="ai-side-card">
                    <h2>What I can help with</h2>

                    <div className="ai-feature">
                        <span>📊</span>
                        <div>
                            <strong>Agency insights</strong>
                            <p>
                                Understand your agency workflow and
                                operations.
                            </p>
                        </div>
                    </div>

                    <div className="ai-feature">
                        <span>📁</span>
                        <div>
                            <strong>Projects</strong>
                            <p>
                                Get guidance for managing projects and
                                project workflows.
                            </p>
                        </div>
                    </div>

                    <div className="ai-feature">
                        <span>✅</span>
                        <div>
                            <strong>Tasks</strong>
                            <p>
                                Organize priorities and improve task
                                management.
                            </p>
                        </div>
                    </div>

                    <div className="ai-feature">
                        <span>👥</span>
                        <div>
                            <strong>Clients</strong>
                            <p>
                                Improve client management and agency
                                processes.
                            </p>
                        </div>
                    </div>

                    <div className="ai-security">
                        <strong>🔐 Secure access</strong>
                        <p>
                            AI requests are protected by your AgencyHub
                            authentication.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default AIAssistant;