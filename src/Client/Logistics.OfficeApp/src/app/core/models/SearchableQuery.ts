import {PagedQuery} from "./PagedQuery";

export interface SearchableQuery extends PagedQuery {
  search?: string;
}
