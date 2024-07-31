#include <Track/TrackFramework.h>
#include <Track/Common.h>

/**
 * @brief Convert from C++ string to C string
 * @param string C++ string
 * @return C string
 */
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    char* res = (char*) malloc (strlen(string) + 1 );
    strcpy (res, string);
    return res;
}

/**
 * @brief Tracking result structure
 */
struct TrackerResult
{
    char* mTargetName = nullptr; ///< Target Name
    uint16_t mFrameID = 0;                                                       ///< FrameID
    double mTranslation[3] = { 0, 0, 0 };                                        ///< Position in relation to camera in meter
    double mRotationMatrix[9] = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 }; ///< 3x3 rotation matrix
    TargetType mType = TargetType::UNDEFINED;                  ///< Target types
    TargetStatus mStatus = TargetStatus::NOT_TRACKED;        ///< Tracking Status
};

/**
 * @brief Vector3 structure
 */
struct Vec3
{
    double X; ///< X component
    double Y; ///< Y component
    double Z; ///< Z component
    
    /**
     * @brief Vector3 constructor
     * @param x X component
     * @param y Y component
     * @param z Z component
     */
    Vec3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
};

/**
 * @brief Size in 3 dimension
 */
struct Dimensions
{
    Vec3 mMin = Vec3(0.0, 0.0, 0.0); ///< Min component
    Vec3 mMax = Vec3(0.0, 0.0, 0.0); ///< Max component
    
    /**
     * @brief Dimensions structure
     * @param min Min component
     * @param max Max component
     */
    Dimensions(Vec3 min, Vec3 max)
    {
        mMin = min;
        mMax = max;
    }
};

/**
 * @brief Intrinsic camera calibration
 */
struct Calibration
{
    int32_t mResolution[2]; ///< Image resolution. Used during calibration
    double mFocalLength[2]; ///< Focal length of the lens
    double mPrincipalPoint[2]; ///< Position of point without distortion
    double mDistortion[5]; ///< Lens distortion model (Radian and tangential)
};

/**
 * @brief Target information structure
 */
struct TargetInfo
{
    const char* mTargetName; ///< Target name
    Dimensions mDimensions; ///< Target size
    TargetType mType; ///< Target type
    BOOL mIgnore = false; ///< If the target should be ignored during processing
    BOOL mStatic = false; ///< Whether the target is static or dynamic in the scene
};
