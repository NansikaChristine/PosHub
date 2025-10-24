export class UpdateOrderEventRequestUIDto {
  constructor(
    public orderId: string = '',
    public status: string = '',
    public cancellationReason: string = ''
  ) {}
}
