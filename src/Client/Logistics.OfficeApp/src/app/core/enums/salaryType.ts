import {EnumLike} from './enumLike';

export enum SalaryType {
  None,
  Monthly,
  Weekly,
  ShareOfGross,
}

export const SalaryTypeEnum: EnumLike = {
  None: {value: 0, description: 'None'},
  Monthly: {value: 1, description: 'Monthly'},
  Weekly: {value: 2, description: 'Weekly'},
  ShareOfGross: {value: 3, description: 'Share of gross'},
};
