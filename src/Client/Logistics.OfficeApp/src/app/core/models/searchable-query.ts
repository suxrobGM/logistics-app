import {PagedQuery} from "./paged-query";

export interface SearchableQuery extends PagedQuery {
  search?: string;
}
