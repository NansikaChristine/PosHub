export class ProductDto {
  constructor(
    public posReference: string = '',
    public name: string = '',
    public description: string = '',
    public type: string = '',
    public posVersion: string = '',
    public originalImageUrl: string = '',
    public price: number = 0,
    public inStorePrice?: number,
    public taxRate: number = 0,
    public isTaxIncluded: boolean = true,
    public containsAlcohol: boolean = false,
    public containsTobacco: boolean = false,
    public isBikeFriendly: boolean = false,
    public showOnline: boolean = false,
    public categories: string[] = [],
    public position: number = 0,
    //public nutritionalInfo: NutritionalInfoDto = new NutritionalInfoDto(),
    public dietaryRestriction: string = '',
    public spiciness: string = '',
    public additives: string[] = [],
    public allergens: string[] = [],
    //public serviceAvailability: ServiceAvailabilityDto[] = [],
    public modifierGroups: string[] = [],
   // public selections: SelectionDto[] = []
  ) {}
}