import {PagedQuery} from './pagedQuery';

export interface SearchableQuery extends PagedQuery {
  search?: string;
}
