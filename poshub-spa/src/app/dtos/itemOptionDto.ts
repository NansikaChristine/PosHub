export class ItemOptionDto {
  constructor(
    public quantity: number = 0,
    public price: number = 0,
    public name: string = '',
    public menuModifierGroupId: string = '',
    public taxRateIds: string[] = [],
    public modifierGroupName: string = '',
    public posReference: string = '',
    public parentPosReference: string = '',
    public parentModifierPosReference: string = '',
    public parentMenuModifierGroupId: string = '',
    public parentMenuModifierId: string = '',
    public parentModifierGroupPosReference: string = '',
    public partnerId: string = ''
  ) { }
}
