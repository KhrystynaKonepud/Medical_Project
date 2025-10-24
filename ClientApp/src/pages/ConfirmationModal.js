import React from 'react';

// Кастомний модальний компонент для підтвердження дій та відображення помилок.
const ConfirmationModal = ({ isOpen, message, onConfirm, onCancel }) => {
    // Якщо модалка не відкрита, не відображати нічого.
    if (!isOpen) return null;

    // Визначаємо, чи є це модалка для помилки (якщо onConfirm ідентичний onCancel)
    // Якщо так, то ми показуємо лише одну кнопку "Закрити".
    const isErrorModal = onConfirm === onCancel;

    return (
        <div
            className="modal fade show d-flex align-items-center justify-content-center"
            tabIndex="-1"
            style={{
                display: 'block',
                backgroundColor: 'rgba(0, 0, 0, 0.5)',
                position: 'fixed',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                zIndex: 1050
            }}
            onClick={isErrorModal ? onCancel : undefined}
        >
            {/* Контент модального вікна */}
            <div
                className="modal-dialog"
                style={{ maxWidth: '400px' }}
                onClick={(e) => e.stopPropagation()}
            >
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title text-dark">
                            {isErrorModal ? "Помилка" : "Підтвердження дії"}
                        </h5>
                        {/* Кнопка закриття (x) */}
                        <button type="button" className="btn-close" onClick={onCancel}></button>
                    </div>

                    <div className="modal-body">
                        <p className="text-muted">{message}</p>
                    </div>

                    <div className="modal-footer">
                        {isErrorModal ? (
                            <button
                                onClick={onCancel}
                                className="btn btn-primary"
                            >
                                Закрити
                            </button>
                        ) : (
                            <>
                                <button
                                    onClick={onCancel}
                                    className="btn btn-light"
                                >
                                    Скасувати
                                </button>
                                <button
                                    onClick={onConfirm}
                                    className="btn btn-danger"
                                >
                                    Підтвердити
                                </button>
                            </>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ConfirmationModal;