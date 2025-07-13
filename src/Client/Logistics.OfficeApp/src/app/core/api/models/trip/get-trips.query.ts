import {PagedQuery} from "../paged-query.model";
import {TripStatus} from "./enums";

export interface GetTripsQuery extends PagedQuery {
  name?: string;
  truckNumber?: string;
  status?: TripStatus;
}
