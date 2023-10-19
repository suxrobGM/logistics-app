import {EnumLike} from './enumLike';

export enum LoadStatus {
  Dispatched,
  PickedUp,
  Delivered
}

export const LoadStatusEnum: EnumLike = {
  Dispatched: {value: 0, description: 'Dispatched'},
  PickedUp: {value: 1, description: 'Picked Up'},
  Delivered: {value: 2, description: 'Delivered'},
};
