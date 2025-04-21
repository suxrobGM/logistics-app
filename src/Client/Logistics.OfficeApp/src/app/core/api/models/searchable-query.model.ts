import {PagedQuery} from "./paged-query.model";

export interface SearchableQuery extends PagedQuery {
  search?: string;
}
