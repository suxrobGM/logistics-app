import {PagedQuery} from './PagedQuery_';

export interface SearchableQuery extends PagedQuery {
  search?: string;
}
