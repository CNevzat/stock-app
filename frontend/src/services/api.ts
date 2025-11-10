// API base URL - mobil ve web için
import { getApiBaseUrl } from '../utils/apiConfig';

const API_BASE_URL = getApiBaseUrl();

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
