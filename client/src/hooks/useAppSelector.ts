import { useSelector, useDispatch } from 'react-redux';
import type { RootState } from '../store';
import type { AppDispatch } from '../store';

export const useAppSelector = <TSelected = unknown>(
  selector: (state: RootState) => TSelected
): TSelected => useSelector<RootState, TSelected>(selector);

export const useAppDispatch = () => useDispatch<AppDispatch>();