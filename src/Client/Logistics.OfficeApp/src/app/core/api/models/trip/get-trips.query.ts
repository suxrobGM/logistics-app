import {SearchableQuery} from "../searchable.query";
import {TripStatus} from "./enums";

export interface GetTripsQuery extends SearchableQuery {
  name?: string;
  truckNumber?: string;
  status?: TripStatus;
}
