import {SearchableQuery} from "../searchable-query.model";
import {TripStatus} from "./enums";

export interface GetTripsQuery extends SearchableQuery {
  name?: string;
  truckNumber?: string;
  status?: TripStatus;
}
