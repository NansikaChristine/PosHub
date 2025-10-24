export class AddressComponentsDto {
  constructor(
    public area: string = '',
    public country: string = '',
    public flatNo: string = '',
    public city: string = '',
    public postalCode: string = '',
    public houseNo: string = '',
    public addressLine1: string = '',
    public addressLine2: string = '',
    public state: string = '',
  ) { }
}
