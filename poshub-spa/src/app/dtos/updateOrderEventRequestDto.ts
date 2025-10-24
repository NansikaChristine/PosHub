import { ChargeDto } from "./chargeDto";
import { CustomerDto } from "./customerDto";
import { DeliveryDto } from "./deliveryDto";
import { DiscountDto } from "./discountDto";
import { DriverDto } from "./driverDto";
import { ItemDto } from "./itemDto";
import { PaymentDto } from "./paymentDto";
import { TaxDto } from "./taxDto";

export class UpdateOrderEventRequestDto {
  constructor(
    public notes: string = '',
    public orderNumber: string = '',
    public sourceDeviceType: string = '',
    public timeZone: string = '',
    public estimatedDeliveryTime: string = '',
    public payments: PaymentDto[] = [],
    public subTotal: number = 0,
    public driverStatus: string = '',
    public fulfillmentType: string = '',
    public tableName: string = '',
    public totalTax: string = '',
    public total: string = '',
    public discounts: DiscountDto[] = [],
    public currency: string = '',
    public estimatedPickupTime: string = '',
    public delivery: DeliveryDto = new DeliveryDto(),
    public cancellationReason: string = '',
    public tax: TaxDto[] = [],
    public friendlyId: string = '',
    public placedOn: string = '',
    public isPaid: boolean = false,
    public charges: ChargeDto[] = [],
    public driver: DriverDto = new DriverDto(),
    public isScheduledOrder: boolean = false,
    public tableId: number = 0,
    public partnerId: string = '',
    public sourceName: string = '',
    public items: ItemDto[] = [],
    public customer: CustomerDto = new CustomerDto(),
    public status: string = '',
  ) { }
}

