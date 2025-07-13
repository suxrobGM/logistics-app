export interface CreateTripCommand {
  name: string;
  plannedStart: Date;
  loads?: string[]; // Array of load IDs
}
