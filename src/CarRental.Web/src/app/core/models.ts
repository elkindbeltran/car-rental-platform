export enum VehicleStatus {
  Available = 1,
  Maintenance = 2,
  Retired = 3,
}

export interface Page<T> { items: T[]; page: number; pageSize: number; totalCount: number; }
export interface VehicleSummary { id: string; make: string; model: string; year: number; licensePlate: string; dailyRate: number; currency: string; status: VehicleStatus; }
export interface Vehicle extends VehicleSummary { vin: string; rowVersion: string; }
export interface CustomerSummary { id: string; firstName: string; lastName: string; email: string; isActive: boolean; }
export interface Customer extends CustomerSummary { phone: string | null; rowVersion: string; }
export interface Booking { id: string; customerId: string; vehicleId: string; pickupAtUtc: string; returnAtUtc: string; dailyRate: number; totalAmount: number; currency: string; status: string; }
export interface ProblemDetails { title?: string; detail?: string; status?: number; code?: string; errors?: Record<string, string[]>; traceId?: string; }
export interface VehicleInput { vin?: string; make: string; model: string; year: number; licensePlate: string; dailyRate: number; currency: string; status?: VehicleStatus; rowVersion?: string; }
export interface CustomerInput { firstName: string; lastName: string; email: string; phone: string | null; rowVersion?: string; }
export interface BookingInput { customerId: string; vehicleId: string; pickupAtUtc: string; returnAtUtc: string; dailyRate: number; currency: string; }
