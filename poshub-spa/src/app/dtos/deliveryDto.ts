import { AddressDto } from "./addressDto";

export class DeliveryDto {
  constructor(
    public instructions: string = '',
    public address: AddressDto = new AddressDto(),
    public deliveryType: string = ''
  ) { }
}
