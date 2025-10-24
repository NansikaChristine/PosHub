export class VehicleDto {
  constructor(
    public color: string = '',
    public model: string = '',
    public make: string = '',
    public latitude?: number,  // nullable
    public longitude?: number, // nullable
    public trackingUrl: string = ''
  ) { }
}
