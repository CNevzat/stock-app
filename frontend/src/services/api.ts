const API_BASE_URL = 'http://localhost:5134/';

import {Api} from "../Api";

export interface PaginationQuery{
    pageNumber: number;
    pageSize: number;
}

const api = new Api({
    baseURL: API_BASE_URL,
    secure: true,
    timeout: 20000,
});

export default api
