import { ItemOptionDto } from "./itemOptionDto";

export class ItemDto {
  constructor(
    public quantity: number = 0,
    public price: number = 0,
    public name: string = '',
    public posReference: string = '',
    public partnerId: string = '',
    public parentPosReference: string = '',
    public menuCategoryId: string = '',
    public taxRateIds: string[] = [],
    public options: ItemOptionDto[] = [],
    public customerNotes: string = ''
  ) { }
}
