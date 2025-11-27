"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AppointmentStatus = void 0;
// Appointment
var AppointmentStatus;
(function (AppointmentStatus) {
    AppointmentStatus[AppointmentStatus["Scheduled"] = 0] = "Scheduled";
    AppointmentStatus[AppointmentStatus["Completed"] = 1] = "Completed";
    AppointmentStatus[AppointmentStatus["Canceled"] = 2] = "Canceled";
})(AppointmentStatus || (exports.AppointmentStatus = AppointmentStatus = {}));
