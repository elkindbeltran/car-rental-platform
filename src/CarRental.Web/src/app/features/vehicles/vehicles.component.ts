import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../core/api.service';
import { AuthzService } from '../../core/authz.service';
import { apiErrorMessage } from '../../core/error-message';
import { Vehicle, VehicleStatus, VehicleSummary } from '../../core/models';

@Component({
  selector: 'app-vehicles',
  imports: [AsyncPipe, CurrencyPipe, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page hero-band">
      <div><span class="eyebrow">CURATED FOR THE ROAD</span><h1>Find your perfect drive.</h1><p>Reliable cars, transparent pricing, and a booking that takes minutes.</p></div>
      @if (authz.isAdmin$ | async) { <button class="button dark" (click)="openCreate()">＋ Add vehicle</button> }
    </section>
    <section class="page catalog">
      <div class="toolbar">
        <label class="search"><span>⌕</span><input [formControl]="search" placeholder="Search make, model, or plate" aria-label="Search vehicles"></label>
        <div class="filters" aria-label="Vehicle status filter">
          @for (filter of filters; track filter.value) { <button [class.active]="status() === filter.value" (click)="setStatus(filter.value)">{{ filter.label }}</button> }
        </div>
        <span class="count">{{ total() }} {{ total() === 1 ? 'vehicle' : 'vehicles' }}</span>
      </div>
      @if (error()) { <div class="notice error">{{ error() }} <button (click)="load()">Try again</button></div> }
      @if (loading()) { <div class="vehicle-grid">@for (item of skeletons; track $index) { <div class="skeleton card"></div> }</div> }
      @else if (!vehicles().length) {
        <div class="empty"><span>⌁</span><h2>No cars found</h2><p>Try another search or clear your filters.</p><button class="button secondary" (click)="clearFilters()">Clear filters</button></div>
      } @else {
        <div class="vehicle-grid">
          @for (vehicle of vehicles(); track vehicle.id; let i = $index) {
            <article class="vehicle-card">
              <div class="vehicle-art" [class]="'vehicle-art tone-' + (i % 4)">
                <span class="status" [class.available]="vehicle.status === VehicleStatus.Available">{{ statusLabel(vehicle.status) }}</span>
                <div class="car-silhouette"><div></div><i></i><i></i></div>
                <span class="plate">{{ vehicle.licensePlate }}</span>
              </div>
              <div class="vehicle-body">
                <span class="meta">{{ vehicle.year }} · {{ vehicle.currency }}</span>
                <h2>{{ vehicle.make }} {{ vehicle.model }}</h2>
                <div class="rate"><strong>{{ vehicle.dailyRate | currency:vehicle.currency:'symbol':'1.0-0' }}</strong><span>/ day</span></div>
                <div class="actions">
                  @if (vehicle.status === VehicleStatus.Available) { <a class="button primary" [routerLink]="['/book']" [queryParams]="{ vehicle: vehicle.id }">Book now <span>→</span></a> }
                  @else { <button class="button disabled" disabled>Unavailable</button> }
                  @if (authz.isAdmin$ | async) { <button class="icon-action" (click)="openEdit(vehicle)" title="Edit vehicle">•••</button> }
                </div>
              </div>
            </article>
          }
        </div>
        @if (totalPages() > 1) { <div class="pagination"><button [disabled]="page() === 1" (click)="goTo(page()-1)">←</button><span>Page {{ page() }} of {{ totalPages() }}</span><button [disabled]="page() === totalPages()" (click)="goTo(page()+1)">→</button></div> }
      }
    </section>
    @if (modalOpen()) {
      <div class="modal-backdrop" (click)="closeModal()">
        <section class="modal" (click)="$event.stopPropagation()" role="dialog" aria-modal="true" aria-labelledby="vehicle-form-title">
          <button class="modal-close" (click)="closeModal()">×</button>
          <span class="eyebrow">FLEET MANAGEMENT</span><h2 id="vehicle-form-title">{{ editingId() ? 'Edit vehicle' : 'Add a new vehicle' }}</h2>
          <form [formGroup]="form" (ngSubmit)="save()">
            @if (!editingId()) { <label class="full">VIN<input formControlName="vin" maxlength="17" placeholder="17-character VIN"><small>Letters I, O, and Q are not valid in a VIN.</small></label> }
            <label>Make<input formControlName="make" placeholder="e.g. Volvo"></label><label>Model<input formControlName="model" placeholder="e.g. XC40"></label>
            <label>Year<input formControlName="year" type="number"></label><label>License plate<input formControlName="licensePlate" placeholder="ABC-123"></label>
            <label>Daily rate<input formControlName="dailyRate" type="number" min="1"></label><label>Currency<input formControlName="currency" maxlength="3"></label>
            @if (editingId()) { <label class="full">Status<select formControlName="status"><option [ngValue]="VehicleStatus.Available">Available</option><option [ngValue]="VehicleStatus.Maintenance">Maintenance</option><option [ngValue]="VehicleStatus.Retired">Retired</option></select></label> }
            @if (formError()) { <div class="notice error full">{{ formError() }}</div> }
            <div class="form-actions full">
              @if (editingId()) { <button type="button" class="button danger" (click)="remove()">Delete</button> }
              <button type="button" class="button secondary" (click)="closeModal()">Cancel</button><button class="button primary" [disabled]="form.invalid || saving()">{{ saving() ? 'Saving…' : 'Save vehicle' }}</button>
            </div>
          </form>
        </section>
      </div>
    }
    @if (success()) { <div class="toast">✓ {{ success() }}</div> }
  `,
  styleUrl: './vehicles.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VehiclesComponent {
  private readonly api = inject(ApiService); private readonly fb = inject(FormBuilder); private readonly destroyRef = inject(DestroyRef);
  readonly authz = inject(AuthzService); readonly VehicleStatus = VehicleStatus; readonly skeletons = Array(6);
  readonly filters = [{label:'All',value:undefined},{label:'Available',value:VehicleStatus.Available},{label:'Maintenance',value:VehicleStatus.Maintenance},{label:'Retired',value:VehicleStatus.Retired}];
  readonly vehicles = signal<VehicleSummary[]>([]); readonly loading = signal(true); readonly error = signal(''); readonly success = signal(''); readonly total = signal(0); readonly page = signal(1); readonly status = signal<VehicleStatus|undefined>(undefined); readonly modalOpen = signal(false); readonly editingId = signal<string|null>(null); readonly rowVersion = signal(''); readonly saving = signal(false); readonly formError = signal('');
  readonly search = this.fb.nonNullable.control('');
  readonly form = this.fb.group({ vin:['',[Validators.required,Validators.pattern(/^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$/)]], make:['',Validators.required], model:['',Validators.required], year:[new Date().getFullYear(),[Validators.required,Validators.min(1886),Validators.max(2200)]], licensePlate:['',Validators.required], dailyRate:[80,[Validators.required,Validators.min(.01)]], currency:['USD',[Validators.required,Validators.pattern(/^[A-Za-z]{3}$/)]], status:[VehicleStatus.Available,Validators.required] });
  constructor(){ this.search.valueChanges.pipe(debounceTime(350),distinctUntilChanged(),takeUntilDestroyed(this.destroyRef)).subscribe(()=>{this.page.set(1);this.load();}); this.load(); }
  totalPages(){return Math.ceil(this.total()/12)} statusLabel(s:VehicleStatus){return VehicleStatus[s]}
  load(){this.loading.set(true);this.error.set('');this.api.vehicles(this.page(),12,this.search.value,this.status()).subscribe({next:r=>{this.vehicles.set(r.items);this.total.set(r.totalCount);this.loading.set(false)},error:e=>{this.error.set(apiErrorMessage(e));this.loading.set(false)}})}
  setStatus(value:VehicleStatus|undefined){this.status.set(value);this.page.set(1);this.load()} clearFilters(){this.search.setValue('');this.setStatus(undefined)} goTo(page:number){this.page.set(page);this.load();window.scrollTo({top:200,behavior:'smooth'})}
  openCreate(){this.editingId.set(null);this.form.reset({vin:'',make:'',model:'',year:new Date().getFullYear(),licensePlate:'',dailyRate:80,currency:'USD',status:VehicleStatus.Available});this.modalOpen.set(true)}
  openEdit(summary:VehicleSummary){this.formError.set('');this.api.vehicle(summary.id).subscribe({next:v=>this.populate(v),error:e=>this.error.set(apiErrorMessage(e))})}
  private populate(v:Vehicle){this.editingId.set(v.id);this.rowVersion.set(v.rowVersion);this.form.patchValue(v);this.form.controls.vin.clearValidators();this.form.controls.vin.updateValueAndValidity();this.modalOpen.set(true)}
  closeModal(){this.modalOpen.set(false);this.formError.set('');this.form.controls.vin.setValidators([Validators.required,Validators.pattern(/^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$/)]);this.form.controls.vin.updateValueAndValidity()}
  save(){if(this.form.invalid)return;this.saving.set(true);this.formError.set('');const raw=this.form.getRawValue();const input={...raw,vin:raw.vin || undefined,make:raw.make!,model:raw.model!,year:raw.year!,licensePlate:raw.licensePlate!,dailyRate:raw.dailyRate!,currency:raw.currency!.toUpperCase(),status:raw.status!,rowVersion:this.rowVersion()};const request=this.editingId()?this.api.updateVehicle(this.editingId()!,input):this.api.createVehicle(input);request.subscribe({next:()=>this.done('Vehicle saved successfully.'),error:e=>{this.formError.set(apiErrorMessage(e));this.saving.set(false)}})}
  remove(){if(!this.editingId()||!confirm('Delete this vehicle from the active fleet?'))return;this.saving.set(true);this.api.deleteVehicle(this.editingId()!,this.rowVersion()).subscribe({next:()=>this.done('Vehicle removed from the fleet.'),error:e=>{this.formError.set(apiErrorMessage(e));this.saving.set(false)}})}
  private done(message:string){this.saving.set(false);this.closeModal();this.success.set(message);this.load();setTimeout(()=>this.success.set(''),3500)}
}
