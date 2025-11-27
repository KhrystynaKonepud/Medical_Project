import dotenv from 'dotenv';
import path from 'path';

// Load environment variables
dotenv.config({ path: path.join(__dirname, '../.env') });

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
