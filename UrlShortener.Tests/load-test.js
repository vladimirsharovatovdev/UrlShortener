import http from 'k6/http';

export const options = {
    // Define the number of iterations for the test
    iterations: 100,
    vus: 50
};

export default function () {
    // Make a GET request to the target URL
    http.get('http://localhost:5158/r/2fhC5g');
}