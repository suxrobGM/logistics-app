import { SearchableQuery } from "../searchable.query";

export interface GetLoadsQuery extends SearchableQuery {
  onlyActiveLoads?: boolean;
}
