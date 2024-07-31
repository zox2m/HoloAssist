#include "Common.h"

#pragma mark - Track Instance
/**
 * @brief Structure for holding all objects that Track instance needed
 */
struct TrackiOSImpl
{
    /**
     * @brief Singleton instance of the TrackiOSImpl
     * @return A reference to the TrackiOSImpl instance
     */
    static TrackiOSImpl& Instance()
    {
        static TrackiOSImpl instance;
        return instance;
    }
    
    /**
     * @brief Constructor
     */
    TrackiOSImpl(){ framework = [[TrackFramework alloc] init]; }
    
    /**
     * @brief Deleted copy Constructor
     */
    TrackiOSImpl(TrackiOSImpl const&) = delete;
    
    /**
     * @brief Deleted assignment operator
     */
    TrackiOSImpl operator=(TrackiOSImpl const&) = delete;
    
    /**
     * @brief Set camera YUV buffer
     * @param data Buffer reference
     * @param width Frame width in pixel
     * @param height Frame height in pixel
     * @param format YUV Format (Semi-Planar or Planar)
     */
    void setCameraYUVBuffers(unsigned char* data, uint16_t width, uint16_t height, uint16_t format)
    {
        frameData = data;
        frameWidth = width;
        frameHeight = height;
        frameFormat = format;
    }
    
    /**
     * @brief Initialize framework
     * @return Success or failure of initialization
     */
    bool initialize()
    {
        isInitialized = [framework initialize:targetLists resourcePath:targetPath];
        return isInitialized;
    }
    
    /**
     * @brief Reset framework
     * @return Success or failure of resetting framework
     */
    bool reset()
    {
        if(!isInitialized) return false;
        
        [framework reset];
        isInitialized = false;
        
        frameData = nullptr;
        frameWidth = 0;
        frameHeight = 0;
        
        [targetLists removeAllObjects];
        targetPath = @"";
        
        return true;
    }
    
    /**
     * @brief Process one frame
     * @return Success or failure of processing
     */
    bool process()
    {
        if(!isInitialized || frameData == nullptr) return false;
        return [framework processWithNativeAddr:frameData width:frameWidth height:frameHeight isYUV:true];
    }
    
    TrackFramework* framework = nullptr; ///< Tracking framework instance
    NSMutableArray<NSString*>* targetLists; ///< Target name list
    NSString* targetPath; ///< Target file path
    bool isInitialized = false; ///< True if track is initialized
    
    unsigned char* frameData = nullptr; ///< Frame data pointer
    uint16_t frameWidth; ///< Frame width
    uint16_t frameHeight; ///< Frame height
    uint16_t frameFormat; ///< YUV format
};

#pragma mark - C interface
extern "C" {

bool initializeFramework()
{
    return TrackiOSImpl::Instance().initialize();
}

bool cleanup()
{
    return TrackiOSImpl::Instance().reset();
}

#pragma mark - Processing
bool process()
{
    return TrackiOSImpl::Instance().process();
}

#pragma mark - Setter
bool setLicenseKey(const char* key)
{
    return [TrackiOSImpl::Instance().framework setLicenseKey:[NSString stringWithCString:key encoding:kCFStringEncodingUTF8]];
}

bool setCameraCalibration(float* intrinscis)
{
    CameraCalibration* calibration = [[CameraCalibration alloc] init];
    calibration.resolution = CGSizeMake(intrinscis[0], intrinscis[1]);
    calibration.focalLength = CGSizeMake(intrinscis[2], intrinscis[3]);
    calibration.principalPoint = CGSizeMake(intrinscis[4], intrinscis[5]);
    
    return [TrackiOSImpl::Instance().framework setCameraCalibration:calibration];
}

bool setTargetDataPath(const char* path)
{
    TrackiOSImpl::Instance().targetPath = [NSString stringWithCString:path encoding:NSUTF8StringEncoding];
    
    return true;
}

bool setTargetInfo(const TargetInfo* targets, uint16_t size)
{
    if(!TrackiOSImpl::Instance().isInitialized) return false;

    for (int32_t i = 0; i < size; i++)
    {
        const TargetInfo& p = targets[i];
      
        TargetInformation *information = [[TargetInformation alloc] init];
        information.targetName = [NSString stringWithCString:p.mTargetName encoding:kCFStringEncodingUTF8];
        information.targetType = p.mType;
        information.targetSize = SPSize3DMake(
                                              p.mDimensions.mMax.X - p.mDimensions.mMin.X,
                                              p.mDimensions.mMax.Y - p.mDimensions.mMin.Y,
                                              p.mDimensions.mMax.Z - p.mDimensions.mMin.Z
                                              );
        information.isIgnored = p.mIgnore;
        information.isStatic = p.mStatic;
        
        if(![TrackiOSImpl::Instance().framework updateTargetInformation:information])
            return false;
    }
    
    return true;
}

bool setTargetNames(const char* targets[], uint16_t size)
{
    TrackiOSImpl::Instance().targetLists = [[NSMutableArray alloc] initWithCapacity:size];
    
    for(int i = 0; i < size; i++)
        [TrackiOSImpl::Instance().targetLists addObject:[NSString stringWithCString:targets[i] encoding:NSUTF8StringEncoding]];
    
    return true;
}

void setDeviceOrientation(DeviceOrientation orientation)
{
    [TrackiOSImpl::Instance().framework setDeviceOrientation:orientation];
}

void setCameraYUVBuffers(unsigned char* data, uint16_t width, uint16_t height, uint16_t format)
{
    TrackiOSImpl::Instance().setCameraYUVBuffers(data,width,height,format);
}

#pragma mark - Getter

const char * getFrameworkVersion()
{
    NSString* version = [TrackiOSImpl::Instance().framework getFrameworkVersion];
    return MakeStringCopy([version UTF8String]);
}

FrameworkState getFrameworkState()
{
    return [TrackiOSImpl::Instance().framework getFrameworkState];
}

DeviceOrientation getDeviceOrientation()
{
    return [TrackiOSImpl::Instance().framework getDeviceOrientation];
}

void getCameraCalibration(Calibration calibration)
{
    CameraCalibration* intrinsics = [TrackiOSImpl::Instance().framework getCameraCalibration];
    calibration.mResolution[0] = intrinsics.resolution.width;
    calibration.mResolution[1] = intrinsics.resolution.height;
    
    calibration.mFocalLength[0] = intrinsics.focalLength.width;
    calibration.mFocalLength[1] = intrinsics.focalLength.height;
    
    calibration.mPrincipalPoint[0] = intrinsics.principalPoint.width;
    calibration.mPrincipalPoint[1] = intrinsics.principalPoint.height;
}

void getTargetInfo(TargetInfo* targets, int32_t size)
{
    if(!TrackiOSImpl::Instance().isInitialized) return;
    
    NSArray<TargetInformation*>* informations = [TrackiOSImpl::Instance().framework getTargetInformations];
    
    for (int32_t i = 0; i < size; i++)
    {
        TargetInfo& p = targets[i];
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"self.targetName == %@", [NSString stringWithCString:p.mTargetName encoding:NSUTF8StringEncoding]];
        NSUInteger index = [informations indexOfObjectPassingTest:^(id obj, NSUInteger idx, BOOL *stop) {
          return [predicate evaluateWithObject:obj];
        }];

        if(index == NSNotFound) continue;
        
        TargetInformation *information = informations[index];
        p.mType = information.targetType;
        p.mDimensions = Dimensions(Vec3(information.targetSize.width * -0.5, information.targetSize.height * -0.5, information.targetSize.depth * -0.5),
                                   Vec3(information.targetSize.width * 0.5, information.targetSize.height * 0.5, information.targetSize.depth * 0.5));
        p.mIgnore = information.isIgnored;
        p.mStatic = information.isStatic;
    }
}

bool getTrackingResult(TrackerResult* targets, int32_t size)
{
    if (size == 0)
    {
        return false;
    }
    else if (targets == nullptr)
    {
        return false;
    }
    
    if(!TrackiOSImpl::Instance().isInitialized) return false;
    
    NSArray<TargetResult *> *results = [TrackiOSImpl::Instance().framework getTrackingResults];
    
    for (int32_t i = 0; i < size; i++)
    {
        TrackerResult& p = targets[i];
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"self.targetName == %@", [NSString stringWithCString:p.mTargetName encoding:NSUTF8StringEncoding]];
        NSUInteger index = [results indexOfObjectPassingTest:^(id obj, NSUInteger idx, BOOL *stop) {
          return [predicate evaluateWithObject:obj];
        }];
        
        if(index == NSNotFound) continue;
        
        simd_float4x4 transform = results[index].transformation.simdTransformation;
        
        p.mTranslation[0] = transform.columns[3][0];
        p.mTranslation[1] = transform.columns[3][1];
        p.mTranslation[2] = transform.columns[3][2];
        
        p.mRotationMatrix[0] = transform.columns[0][0];
        p.mRotationMatrix[1] = transform.columns[1][0];
        p.mRotationMatrix[2] = transform.columns[2][0];
        
        p.mRotationMatrix[3] = transform.columns[0][1];
        p.mRotationMatrix[4] = transform.columns[1][1];
        p.mRotationMatrix[5] = transform.columns[2][1];
        
        p.mRotationMatrix[6] = transform.columns[0][2];
        p.mRotationMatrix[7] = transform.columns[1][2];
        p.mRotationMatrix[8] = transform.columns[2][2];
        
        p.mStatus = results[index].trackingStatus;
    }

    return true;
}
}
