import { AddressComponentsDto } from "./addressComponentsDto";

export class AddressDto {
  constructor(
    public address: string = '',
    public googlePlaceId: string = '',
    public latitude: number = 0,
    public longitude: number = 0,
    public addressComponents: AddressComponentsDto = new AddressComponentsDto(),
  ) { }
}
