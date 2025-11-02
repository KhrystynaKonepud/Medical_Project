import React from 'react';

// Стиль, щоб футер був притиснутий до низу, якщо контенту мало
const footerStyle = {
    backgroundColor: '#343a40',
    color: 'white',
    padding: '1.5rem 0',
    marginTop: 'auto' // Це "притискає" його до низу в flex-контейнері
};

export default function DoctorFooter() {
    return (
        <footer style={footerStyle} className="text-center">
            <div className="container">
                &copy; {new Date().getFullYear()} Medical Center. Всі права захищено.
            </div>
        </footer>
    );
}