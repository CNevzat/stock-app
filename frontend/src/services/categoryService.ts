import api, {type PaginationQuery} from "./api.ts";
import type {CreateCategoryCommand, UpdateCategoryCommand} from "../Api";


export const categoryService = {
  getAll: async (params: PaginationQuery & { searchTerm?: string }) => {
    const { data } = await api.api.categoriesList({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        searchTerm: params.searchTerm
    });
    return data;
  },

  getById: async (id: number)=> {
    const { data } = await api.api.categoriesByIdList({
        id
    });
    return data;
  },

  create: async (dto: CreateCategoryCommand)=> {
    const response = await api.api.categoriesCreate(dto);
    return response.data.categoryId;
  },

  update: async (id: number, dto: UpdateCategoryCommand) => {
      const response = await api.api.categoriesUpdate({
          categoryId: id,
          name: dto.name
      });
      
      return response.data
  },

  delete: async (id: number)=> {
    await api.api.categoriesDelete({
        id : id
    });
  },
};

