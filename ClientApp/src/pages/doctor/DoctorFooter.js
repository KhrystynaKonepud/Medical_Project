import React from 'react';

const footerStyle = {
    backgroundColor: '#343a40',
    color: 'white',
    padding: '1.5rem 0',
    marginTop: 'auto'
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