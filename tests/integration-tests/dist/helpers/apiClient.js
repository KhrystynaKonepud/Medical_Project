"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.apiClient = void 0;
const axios_1 = __importDefault(require("axios"));
const https_1 = __importDefault(require("https"));
const config_1 = require("../config");
class ApiClient {
    constructor() {
        this.client = axios_1.default.create({
            baseURL: config_1.config.apiBaseUrl,
            timeout: config_1.config.apiTimeout,
            headers: {
                'Content-Type': 'application/json',
            },
            // Disable SSL verification for local development
            httpsAgent: new https_1.default.Agent({
                rejectUnauthorized: false,
            }),
        });
    }
    async get(url, config) {
        const response = await this.client.get(url, config);
        return response.data;
    }
    async post(url, data, config) {
        const response = await this.client.post(url, data, config);
        return response.data;
    }
    async put(url, data, config) {
        const response = await this.client.put(url, data, config);
        return response.data;
    }
    async delete(url, config) {
        const response = await this.client.delete(url, config);
        return response.data;
    }
    async patch(url, data, config) {
        const response = await this.client.patch(url, data, config);
        return response.data;
    }
    getClient() {
        return this.client;
    }
}
exports.apiClient = new ApiClient();
