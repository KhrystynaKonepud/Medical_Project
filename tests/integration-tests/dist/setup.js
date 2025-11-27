"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const dotenv_1 = __importDefault(require("dotenv"));
const path_1 = __importDefault(require("path"));
// Load environment variables
dotenv_1.default.config({ path: path_1.default.join(__dirname, '../.env') });
// Global test configuration
jest.setTimeout(30000);
// Setup and teardown
beforeAll(() => {
    console.log(`Running tests with DB Provider: ${process.env.DB_PROVIDER || 'SqlServer'}`);
    console.log(`API Base URL: ${process.env.API_BASE_URL || 'https://localhost:7041'}`);
});
afterAll(() => {
    console.log('All tests completed');
});
