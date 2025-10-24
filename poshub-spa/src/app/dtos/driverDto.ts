import { VehicleDto } from "./vehicleDto";

export class DriverDto {
  constructor(
    public firstName: string = '',
    public lastName: string = '',
    public phoneNumber: string = '',
    public driverReference: string = '',
    public vehicle: VehicleDto = new VehicleDto()
  ) { }
}
