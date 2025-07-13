export interface UpdateTripCommand {
  tripId: string;
  name?: string;
  plannedStart?: Date;
  loads?: string[]; // Array of load IDs
}
