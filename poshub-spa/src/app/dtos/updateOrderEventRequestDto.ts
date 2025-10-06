export class UpdateOrderEventRequestDto {
  constructor(
    public orderId: string = '',
    public status: string = '',
    public cancellationReason: string = ''
  ) {}
}
