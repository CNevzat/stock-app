/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

/** @format int32 */
export enum TodoStatus {
  Value1 = 1,
  Value2 = 2,
  Value3 = 3,
}

/** @format int32 */
export enum TodoPriority {
  Value1 = 1,
  Value2 = 2,
  Value3 = 3,
}

/** @format int32 */
export enum StockMovementType {
  Value1 = 1,
  Value2 = 2,
}

export interface CategoryDto {
  /** @format int32 */
  id?: number;
  name?: string | null;
  /** @format int32 */
  productCount?: number;
}

export interface CategoryDtoPaginatedList {
  items?: CategoryDto[] | null;
  /** @format int32 */
  pageNumber?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  totalCount?: number;
  /** @format int32 */
  totalPages?: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
}

export interface CategoryStatsDto {
  /** @format int32 */
  categoryId?: number;
  categoryName?: string | null;
  /** @format int32 */
  productCount?: number;
  /** @format int32 */
  totalStock?: number;
}

export interface CreateCategoryCommand {
  name?: string | null;
}

export interface CreateCategoryCommandResponse {
  /** @format int32 */
  categoryId?: number;
}

export interface CreateProductAttributeCommand {
  /** @format int32 */
  productId?: number;
  key?: string | null;
  value?: string | null;
}

export interface CreateProductAttributeCommandResponse {
  /** @format int32 */
  productAttributeId?: number;
}

export interface CreateProductCommand {
  name?: string | null;
  description?: string | null;
  /** @format int32 */
  stockQuantity?: number;
  /** @format int32 */
  lowStockThreshold?: number;
  /** @format int32 */
  categoryId?: number;
}

export interface CreateProductCommandResponse {
  /** @format int32 */
  productId?: number;
}

export interface CreateStockMovementCommand {
  /** @format int32 */
  productId?: number;
  type?: StockMovementType;
  /** @format int32 */
  quantity?: number;
  description?: string | null;
}

export interface CreateTodoCommand {
  title?: string | null;
  description?: string | null;
  status?: TodoStatus;
  priority?: TodoPriority;
}

export interface DashboardStatsDto {
  /** @format int32 */
  totalCategories?: number;
  /** @format int32 */
  totalProducts?: number;
  /** @format int32 */
  totalProductAttributes?: number;
  /** @format int32 */
  totalStockQuantity?: number;
  /** @format int32 */
  lowStockProducts?: number;
  /** @format int32 */
  outOfStockProducts?: number;
  categoryStats?: CategoryStatsDto[] | null;
  productStockStatus?: ProductStockDto[] | null;
  stockDistribution?: StockDistributionDto[] | null;
  recentStockMovements?: RecentStockMovementDto[] | null;
}

export interface DeleteCategoryCommandResponse {
  /** @format int32 */
  categoryId?: number;
}

export interface DeleteProductAttributeCommandResponse {
  /** @format int32 */
  productAttributeId?: number;
}

export interface DeleteProductCommandResponse {
  /** @format int32 */
  productId?: number;
}

export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  /** @format int32 */
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  [key: string]: any;
}

export interface ProductAttributeDto {
  /** @format int32 */
  id?: number;
  /** @format int32 */
  productId?: number;
  productName?: string | null;
  key?: string | null;
  value?: string | null;
}

export interface ProductAttributeDtoPaginatedList {
  items?: ProductAttributeDto[] | null;
  /** @format int32 */
  pageNumber?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  totalCount?: number;
  /** @format int32 */
  totalPages?: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
}

export interface ProductDto {
  /** @format int32 */
  id?: number;
  name?: string | null;
  stockCode?: string | null;
  description?: string | null;
  /** @format int32 */
  stockQuantity?: number;
  /** @format int32 */
  lowStockThreshold?: number;
  /** @format int32 */
  categoryId?: number;
  categoryName?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string | null;
}

export interface ProductDtoPaginatedList {
  items?: ProductDto[] | null;
  /** @format int32 */
  pageNumber?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  totalCount?: number;
  /** @format int32 */
  totalPages?: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
}

export interface ProductStockDto {
  /** @format int32 */
  productId?: number;
  productName?: string | null;
  stockCode?: string | null;
  /** @format int32 */
  stockQuantity?: number;
  categoryName?: string | null;
  status?: string | null;
}

export interface RecentStockMovementDto {
  /** @format int32 */
  id?: number;
  productName?: string | null;
  categoryName?: string | null;
  type?: StockMovementType;
  typeText?: string | null;
  /** @format int32 */
  quantity?: number;
  description?: string | null;
  /** @format date-time */
  createdAt?: string;
}

export interface StockDistributionDto {
  status?: string | null;
  /** @format int32 */
  count?: number;
  /** @format int32 */
  percentage?: number;
}

export interface UpdateCategoryCommand {
  /** @format int32 */
  categoryId?: number;
  name?: string | null;
}

export interface UpdateCategoryCommandResponse {
  /** @format int32 */
  categoryId?: number;
}

export interface UpdateProductAttributeCommand {
  /** @format int32 */
  id?: number;
  key?: string | null;
  value?: string | null;
}

export interface UpdateProductAttributeCommandResponse {
  /** @format int32 */
  productAttributeId?: number;
}

export interface UpdateProductCommand {
  /** @format int32 */
  id?: number;
  name?: string | null;
  description?: string | null;
  /** @format int32 */
  stockQuantity?: number | null;
  /** @format int32 */
  lowStockThreshold?: number | null;
}

export interface UpdateProductCommandResponse {
  /** @format int32 */
  productId?: number;
}

export interface UpdateTodoCommand {
  /** @format int32 */
  id?: number;
  title?: string | null;
  description?: string | null;
  status?: TodoStatus;
  priority?: TodoPriority;
}

import type {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  HeadersDefaults,
  ResponseType,
} from "axios";
import axios from "axios";

export type QueryParamsType = Record<string | number, any>;

export interface FullRequestParams
  extends Omit<AxiosRequestConfig, "data" | "params" | "url" | "responseType"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseType;
  /** request body */
  body?: unknown;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown>
  extends Omit<AxiosRequestConfig, "data" | "cancelToken"> {
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<AxiosRequestConfig | void> | AxiosRequestConfig | void;
  secure?: boolean;
  format?: ResponseType;
}

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public instance: AxiosInstance;
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private secure?: boolean;
  private format?: ResponseType;

  constructor({
    securityWorker,
    secure,
    format,
    ...axiosConfig
  }: ApiConfig<SecurityDataType> = {}) {
    this.instance = axios.create({
      ...axiosConfig,
      baseURL: axiosConfig.baseURL || "",
    });
    this.secure = secure;
    this.format = format;
    this.securityWorker = securityWorker;
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected mergeRequestParams(
    params1: AxiosRequestConfig,
    params2?: AxiosRequestConfig,
  ): AxiosRequestConfig {
    const method = params1.method || (params2 && params2.method);

    return {
      ...this.instance.defaults,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...((method &&
          this.instance.defaults.headers[
            method.toLowerCase() as keyof HeadersDefaults
          ]) ||
          {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected stringifyFormItem(formItem: unknown) {
    if (typeof formItem === "object" && formItem !== null) {
      return JSON.stringify(formItem);
    } else {
      return `${formItem}`;
    }
  }

  protected createFormData(input: Record<string, unknown>): FormData {
    if (input instanceof FormData) {
      return input;
    }
    return Object.keys(input || {}).reduce((formData, key) => {
      const property = input[key];
      const propertyContent: any[] =
        property instanceof Array ? property : [property];

      for (const formItem of propertyContent) {
        const isFileType = formItem instanceof Blob || formItem instanceof File;
        formData.append(
          key,
          isFileType ? formItem : this.stringifyFormItem(formItem),
        );
      }

      return formData;
    }, new FormData());
  }

  public request = async <T = any, _E = any>({
    secure,
    path,
    type,
    query,
    format,
    body,
    ...params
  }: FullRequestParams): Promise<AxiosResponse<T>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const responseFormat = format || this.format || undefined;

    if (
      type === ContentType.FormData &&
      body &&
      body !== null &&
      typeof body === "object"
    ) {
      body = this.createFormData(body as Record<string, unknown>);
    }

    if (
      type === ContentType.Text &&
      body &&
      body !== null &&
      typeof body !== "string"
    ) {
      body = JSON.stringify(body);
    }

    return this.instance.request({
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type ? { "Content-Type": type } : {}),
      },
      params: query,
      responseType: responseFormat,
      data: body,
      url: path,
    });
  };
}

/**
 * @title Stock App API
 * @version v1
 *
 * Stock Management API with CQRS and Pagination
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Category
     * @name CategoriesList
     * @request GET:/api/categories
     */
    categoriesList: (
      query?: {
        /**
         * @format int32
         * @default 1
         */
        pageNumber?: number;
        /**
         * @format int32
         * @default 10
         */
        pageSize?: number;
        searchTerm?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<CategoryDtoPaginatedList, any>({
        path: `/api/categories`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Category
     * @name CategoriesCreate
     * @request POST:/api/categories
     */
    categoriesCreate: (
      data: CreateCategoryCommand,
      params: RequestParams = {},
    ) =>
      this.request<CreateCategoryCommandResponse, any>({
        path: `/api/categories`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Category
     * @name CategoriesUpdate
     * @request PUT:/api/categories
     */
    categoriesUpdate: (
      data: UpdateCategoryCommand,
      params: RequestParams = {},
    ) =>
      this.request<UpdateCategoryCommandResponse, ProblemDetails>({
        path: `/api/categories`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Category
     * @name CategoriesDelete
     * @request DELETE:/api/categories
     */
    categoriesDelete: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<DeleteCategoryCommandResponse, ProblemDetails>({
        path: `/api/categories`,
        method: "DELETE",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Category
     * @name CategoriesByIdList
     * @request GET:/api/categories/by-id
     */
    categoriesByIdList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<CategoryDto, ProblemDetails>({
        path: `/api/categories/by-id`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Dashboard
     * @name DashboardStatsList
     * @request GET:/api/Dashboard/stats
     */
    dashboardStatsList: (params: RequestParams = {}) =>
      this.request<DashboardStatsDto, any>({
        path: `/api/Dashboard/stats`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Product
     * @name ProductsList
     * @request GET:/api/products
     */
    productsList: (
      query?: {
        /**
         * @format int32
         * @default 1
         */
        pageNumber?: number;
        /**
         * @format int32
         * @default 10
         */
        pageSize?: number;
        /** @format int32 */
        categoryId?: number;
        searchTerm?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<ProductDtoPaginatedList, any>({
        path: `/api/products`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Product
     * @name ProductsCreate
     * @request POST:/api/products
     */
    productsCreate: (data: CreateProductCommand, params: RequestParams = {}) =>
      this.request<CreateProductCommandResponse, any>({
        path: `/api/products`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Product
     * @name ProductsUpdate
     * @request PUT:/api/products
     */
    productsUpdate: (data: UpdateProductCommand, params: RequestParams = {}) =>
      this.request<UpdateProductCommandResponse, ProblemDetails>({
        path: `/api/products`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Product
     * @name ProductsDelete
     * @request DELETE:/api/products
     */
    productsDelete: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<DeleteProductCommandResponse, ProblemDetails>({
        path: `/api/products`,
        method: "DELETE",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Product
     * @name ProductsByIdList
     * @request GET:/api/products/by-id
     */
    productsByIdList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<ProductDto, ProblemDetails>({
        path: `/api/products/by-id`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ProductAttribute
     * @name ProductAttributesList
     * @request GET:/api/product-attributes
     */
    productAttributesList: (
      query?: {
        /**
         * @format int32
         * @default 1
         */
        pageNumber?: number;
        /**
         * @format int32
         * @default 10
         */
        pageSize?: number;
        /** @format int32 */
        productId?: number;
        searchKey?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<ProductAttributeDtoPaginatedList, any>({
        path: `/api/product-attributes`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ProductAttribute
     * @name ProductAttributesCreate
     * @request POST:/api/product-attributes
     */
    productAttributesCreate: (
      data: CreateProductAttributeCommand,
      params: RequestParams = {},
    ) =>
      this.request<CreateProductAttributeCommandResponse, any>({
        path: `/api/product-attributes`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ProductAttribute
     * @name ProductAttributesUpdate
     * @request PUT:/api/product-attributes
     */
    productAttributesUpdate: (
      data: UpdateProductAttributeCommand,
      params: RequestParams = {},
    ) =>
      this.request<UpdateProductAttributeCommandResponse, ProblemDetails>({
        path: `/api/product-attributes`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ProductAttribute
     * @name ProductAttributesDelete
     * @request DELETE:/api/product-attributes
     */
    productAttributesDelete: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<DeleteProductAttributeCommandResponse, ProblemDetails>({
        path: `/api/product-attributes`,
        method: "DELETE",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ProductAttribute
     * @name ProductAttributesByIdList
     * @request GET:/api/product-attributes/by-id
     */
    productAttributesByIdList: (
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<ProductAttributeDto, ProblemDetails>({
        path: `/api/product-attributes/by-id`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StockMovement
     * @name StockMovementList
     * @request GET:/api/StockMovement
     */
    stockMovementList: (
      query?: {
        /**
         * @format int32
         * @default 1
         */
        pageNumber?: number;
        /**
         * @format int32
         * @default 10
         */
        pageSize?: number;
        /** @format int32 */
        productId?: number;
        /** @format int32 */
        categoryId?: number;
        type?: StockMovementType;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/StockMovement`,
        method: "GET",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags StockMovement
     * @name StockMovementCreate
     * @request POST:/api/StockMovement
     */
    stockMovementCreate: (
      data: CreateStockMovementCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/StockMovement`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Todo
     * @name TodoList
     * @request GET:/api/Todo
     */
    todoList: (
      query?: {
        /**
         * @format int32
         * @default 1
         */
        pageNumber?: number;
        /**
         * @format int32
         * @default 10
         */
        pageSize?: number;
        status?: TodoStatus;
        priority?: TodoPriority;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/Todo`,
        method: "GET",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Todo
     * @name TodoCreate
     * @request POST:/api/Todo
     */
    todoCreate: (data: CreateTodoCommand, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Todo`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Todo
     * @name TodoUpdate
     * @request PUT:/api/Todo/{id}
     */
    todoUpdate: (
      id: number,
      data: UpdateTodoCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/Todo/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Todo
     * @name TodoDelete
     * @request DELETE:/api/Todo/{id}
     */
    todoDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Todo/${id}`,
        method: "DELETE",
        ...params,
      }),
  };
}

export const api = new Api({ baseURL: "http://localhost:5134" });
export default api;
