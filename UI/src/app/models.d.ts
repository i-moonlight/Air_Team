export interface ImageDto {
    imageId: string;
    title: string;
    description?: string;
    baseImageUrl?: string;
    detailUrl?: string;
}

declare global {
    interface Window {
        Api_URL: string;
    }
}
