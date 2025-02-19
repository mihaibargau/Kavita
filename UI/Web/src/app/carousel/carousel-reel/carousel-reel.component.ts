import { Component, ContentChild, EventEmitter, Input, Output, TemplateRef } from '@angular/core';
import { Swiper, SwiperEvents } from 'swiper/types';

@Component({
  selector: 'app-carousel-reel',
  templateUrl: './carousel-reel.component.html',
  styleUrls: ['./carousel-reel.component.scss']
})
export class CarouselReelComponent {

  @ContentChild('carouselItem') carouselItemTemplate!: TemplateRef<any>;
  @Input() items: any[] = [];
  @Input() title = '';
  @Input() clickableTitle: boolean = true;
  @Output() sectionClick = new EventEmitter<string>();

  swiper: Swiper | undefined;

  trackByIdentity: (index: number, item: any) => string;

  constructor() { 
    this.trackByIdentity = (index: number, item: any) => `${this.title}_${item.id}_${item?.name}_${item?.pagesRead}_${index}`;
  }

  nextPage() {
    if (this.swiper) {
      if (this.swiper.isEnd) return;
      this.swiper.setProgress(this.swiper.progress + 0.25, 600);
    }
  }

  prevPage() {
    if (this.swiper) {
      if (this.swiper.isBeginning) return;
      this.swiper.setProgress(this.swiper.progress - 0.25, 600);
    }
  }

  sectionClicked(event: any) {
    this.sectionClick.emit(this.title);
  }

  onSwiper(eventParams: Parameters<SwiperEvents['init']>) {
    [this.swiper] = eventParams;
  }
}
